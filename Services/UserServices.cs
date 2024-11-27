using Microsoft.AspNetCore.Mvc;
using MyPortal.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using NAV;
using Microsoft.OData;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using System.IO.Compression;

namespace MyPortal.Services
{
    public class UserServices
    {
        private static List<ClientUser> cachedUsers;
        private static string accessToken;
        private static DateTime tokenExpiry;

        public IConfiguration _configuration { get; }
        public IHttpContextAccessor _context { get; }
        private readonly CustomerServices _customerServices;
        private readonly VendorServices _vendorServices;

        public UserServices(IConfiguration configuration, IHttpContextAccessor context, CustomerServices customerServices, VendorServices vendorServices)
        {
            _vendorServices = vendorServices;
            _customerServices = customerServices;
            _configuration = configuration;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var tenantId = _configuration.GetSection("AzureAD").GetValue<string>("TenantId");
            var clientId = _configuration.GetSection("AzureAD").GetValue<string>("ClientId");
            var clientSecret = _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret");
            // Get the access token
            accessToken = await GetAccessTokenAsync(tenantId, clientId, clientSecret);

        }
        public static async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(accessToken) || tokenExpiry <= DateTime.UtcNow)
            {
                // Get a new access token
                var newTokenResponse = await TokenManager.GetNewAccessTokenAsync(tenantId, clientId, clientSecret);
                accessToken = newTokenResponse.access_token;
                var currenttime = DateTime.UtcNow;
                tokenExpiry = DateTime.UtcNow.AddSeconds(newTokenResponse.expires_in - 3539); // Token expiry buffer
            }
            return accessToken;
        }

        private async Task<List<ClientUser>> FetchUsersFromService(string accessToken, string Email)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("CustomerLoginUrl");
            string FilterUrl = $"{BCCustomerLoginApiUrl}/?$filter=Email eq '{Email}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<ODataResponse<ClientUser>>(data);
                    return users.Value;
                }
                else
                {
                    Console.WriteLine("Failed to get data: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> LoginUser(string Email, string password)
        {
            // if (string.IsNullOrEmpty(accessToken) || tokenExpiry <= DateTime.UtcNow )
            // {
            //     var token = InitializeAsync();
            //     accessToken = token.ToString();
            // }
            cachedUsers = await FetchUsersFromService(accessToken, Email);
            bool value = false;
            var Users = cachedUsers.Where(u => u.Email == Email).ToList();
            if (Users.Count == 2)
            {
                return new OkObjectResult(new { value = true, UsersData = Users });
            }
            else if (Users.Count == 1)
            {
                ClientUser User = Users[0];
                if (User != null)
                {
                    if (User.Type == "Customer")
                    {
                        if (User.Temporary == false)
                        {
                            if (User.Password == password)
                            {
                                var UserDetails = await GetCustomerDetails(User.No.ToString());

                                if (UserDetails != null)
                                {
                                    UserDetails.Add("UserType", "Customer");
                                    return new OkObjectResult(UserDetails);
                                }
                                else
                                {
                                    return new BadRequestObjectResult("Failed to retrieve user details");
                                }
                            }
                            else
                            {
                                return new UnauthorizedObjectResult("Invalid username or password");
                            }

                        }
                        else
                        {
                            // byte[] emailBytes = Encoding.UTF8.GetBytes(User.Email);
                            // string email = Convert.ToBase64String(emailBytes);
                            return new OkObjectResult(new { redirected = true, UsersData = Users });
                            // return new RedirectToActionResult("Change_Password", "Home", new { email });
                        }

                    }
                    else
                    {
                        if (User.Temporary == false)
                        {
                            if (User.Password == password)
                            {
                                var UserDetails = await GetVendorDetails(User.No.ToString());

                                if (UserDetails != null)
                                {
                                    UserDetails.Add("UserType", "Vendor");
                                    return new OkObjectResult(UserDetails);
                                }
                                else
                                {
                                    return new BadRequestObjectResult("Failed to retrieve user details");
                                }
                            }
                            else
                            {
                                return new UnauthorizedObjectResult("Invalid username or password");
                            }

                        }
                        else
                        {
                            return new OkObjectResult(new { redirected = true, UsersData = Users });
                        }

                    }
                }
                else
                {
                    return new BadRequestObjectResult("User Data not found!");
                }
            }
            else
            {
                return new UnauthorizedObjectResult("Invalid username or password");
            }
        }
        public async Task<Dictionary<string, Object>> GetCustomerDetails(string UniqueNo)
        {
            return await _customerServices.CustomerDetails(UniqueNo, accessToken);
        }
        public async Task<Dictionary<string, Object>> GetVendorDetails(string UniqueNo)
        {

            return await _vendorServices.VendorDetails(UniqueNo, accessToken);
        }
        public async Task<IActionResult> CheckUSerExist(string email)
        {
            cachedUsers = await FetchUsersFromService(accessToken, email);
            if (cachedUsers.Count == 2)
            {
                return new OkObjectResult(new { value = true, UsersData = cachedUsers });
            }
            else
            {
                var IsUserExist = cachedUsers.FirstOrDefault(x => x.Email == email);
                if (IsUserExist != null)
                {
                    return await GetTempPassword(IsUserExist.Type, IsUserExist.Email);

                }
                else
                {
                    return new BadRequestObjectResult("Enter business central email id to change password");
                }
            }
        }

        public async Task<IActionResult> GetTempPassword(string Type, string Email)
        {

            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:GenerateTempPassword";
            int Val = Type == "Customer" ? 0 : 1;
            var SoapXml = $@"
                    <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                    <soapenv:Header/>
                    <soapenv:Body>
                        <urn:GenerateTempPassword>
                            <urn:type_iEnum>{Val}</urn:type_iEnum>
                            <urn:email_iTxt>{Email}</urn:email_iTxt>
                        </urn:GenerateTempPassword>
                    </soapenv:Body>
                    </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml")
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var IsSent = RootElement.Value;
                    return new OkObjectResult(IsSent);
                }
                else
                {
                    return new BadRequestObjectResult("Error while sending temporary password:" + Response.IsSuccessStatusCode);
                }
            }

        }

        public async Task<bool> GetChangePassword(string email, string password, string type)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:ChangePassword";
            int Val = type == "Customer" ? 0 : 1;
            var SoapXml = $@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' 
                            xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                                <soapenv:Header/>
                                <soapenv:Body>
                                    <urn:ChangePassword>
                                        <urn:type_iEnum>{Val}</urn:type_iEnum>
                                        <urn:email_iTxt>{email}</urn:email_iTxt>
                                        <urn:newPass>{password}</urn:newPass>
                                    </urn:ChangePassword>
                                </soapenv:Body>
                            </soapenv:Envelope>";
            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml")
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var IsSent = bool.Parse(RootElement.Value);
                    return IsSent;
                }
                else
                {
                    Console.WriteLine("Error while changing new password:" + Response.IsSuccessStatusCode);
                    return false;
                }
            }

        }
        public async Task<IActionResult> GetCustomerOrders(string No)
        {
            return await _customerServices.CustomerOrders(No, accessToken);
        }
        public async Task<IActionResult> GetVendorOrders(string No)
        {
            return await _vendorServices.VendorOrders(No, accessToken);
        }

        public async Task<IActionResult> GetCustomerInvoices(string No)
        {
            return await _customerServices.CustomerInvoices(No, accessToken);
        }
        public async Task<IActionResult> GetVendorInvoices(string No)
        {
            return await _vendorServices.VendorInvoices(No, accessToken);
        }
        public async Task<IActionResult> GetEarliestPaymentDate(string No)
        {

            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:GetEarliestPaymentDate";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                        <soapenv:Header/>
                        <soapenv:Body>
                            <urn:GetEarliestPaymentDate>
                                <urn:custNo>{No}</urn:custNo>
                            </urn:GetEarliestPaymentDate>
                        </soapenv:Body>
                    </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml")
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Date = RootElement.Value.ToString();
                    return new OkObjectResult(new { Date = Date });
                }
                else
                {
                    Console.WriteLine("Error while sending temporary password:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }

        }
        public async Task<IActionResult> GetEarliestPaymentAmount(string No)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:GetEarliestPaymentAmount";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                        <soapenv:Header/>
                        <soapenv:Body>
                            <urn:GetEarliestPaymentAmount>
                                <urn:custNo>{No}</urn:custNo>
                            </urn:GetEarliestPaymentAmount>
                        </soapenv:Body>
                    </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml")
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Amount = double.Parse(RootElement.Value);
                    return new OkObjectResult(Amount);
                }
                else
                {
                    Console.WriteLine("Error while sending temporary password:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadOrders(string OrderNo)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadSalesOrderReport";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadSalesOrderReport>
                        <urn:salesOrderNo_iCod>{OrderNo}</urn:salesOrderNo_iCod>
                    </urn:DownloadSalesOrderReport>
                </soapenv:Body>
            </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                // Request.Headers.Add("Accept", "application/pdf");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadInvoices(string InvoiceNo)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadSalesInvoiceReport";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                        <soapenv:Header/>
                        <soapenv:Body>
                            <urn:DownloadSalesInvoiceReport>
                                <urn:salesInvoiceNo_iCod>{InvoiceNo}</urn:salesInvoiceNo_iCod>
                            </urn:DownloadSalesInvoiceReport>
                        </soapenv:Body>
                    </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadStatements(string No, DateRange DateRange)
        {
            var StartDate = DateRange.StartDate.ToString("yyyy-MM-dd");
            var EndDate = DateRange.EndDate.ToString("yyyy-MM-dd");
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadCustomerStatement";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                    <soapenv:Body>
                        <urn:DownloadCustomerStatement>
                            <urn:custNo>{No}</urn:custNo>
                            <urn:startDate>{StartDate}</urn:startDate>
                            <urn:endDate>{EndDate}</urn:endDate>
                        </urn:DownloadCustomerStatement>
                    </soapenv:Body>
            </soapenv:Envelope>
            ";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> GetCustomerQuates(string No)
        {
            return await _customerServices.CustomerQuates(No, accessToken);
        }
        public async Task<IActionResult> GetVendorQuates(string No)
        {
            return await _vendorServices.VendorQuates(No, accessToken);
        }

        public async Task<IActionResult> DownloadQuates(string QuateNo)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadSalesQuoteReport";
            var SoapXml = $@"
           <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadSalesQuoteReport>
                        <urn:salesQuoteNo_iCod>{QuateNo}</urn:salesQuoteNo_iCod>
                    </urn:DownloadSalesQuoteReport>
                </soapenv:Body>
            </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                // Request.Headers.Add("Accept", "application/pdf");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> GetCustomerSalesCreditMemo(string No)
        {
            return await _customerServices.CustomerSalesCreditMemo(No, accessToken);
        }
        public async Task<IActionResult> GetVendorSalesCreditMemo(string No)
        {
            return await _vendorServices.VendorSalesCreditMemo(No, accessToken);
        }

        public async Task<IActionResult> DownloadSalesCreditMemos(string SalesCreditMemoNo)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadSalesCreditMemoReport";
            var SoapXml = $@"
           <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadSalesCreditMemoReport>
                        <urn:salesCreditMemoNo_iCod>{SalesCreditMemoNo}</urn:salesCreditMemoNo_iCod>
                    </urn:DownloadSalesCreditMemoReport>
                </soapenv:Body>
            </soapenv:Envelope>";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                // Request.Headers.Add("Accept", "application/pdf");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadVendorDetails(string No)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadVendorDetails";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                    <soapenv:Body>
                        <urn:DownloadVendorDetails>
                            <urn:vendorNo>{No}</urn:vendorNo>
                        </urn:DownloadVendorDetails>
                    </soapenv:Body>
            </soapenv:Envelope>
            ";


            using (HttpClient client = new HttpClient())
            {
                var Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BCCustomerLoginApiUrl),
                    Content = new StringContent(SoapXml, Encoding.UTF8, "text/xml"),
                };
                Request.Headers.Add("SOAPAction", SOAPAction);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage Response = await client.SendAsync(Request);

                if (Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadAsStringAsync();
                    var RootElement = XElement.Parse(Data);
                    var Base64String = RootElement.Value;
                    return new OkObjectResult(new { base64 = Base64String });
                }
                else
                {
                    Console.WriteLine("Error while downloading order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }
    }
}
public class UserInfo
{
    public IActionResult UserDetails { get; set; }
    public string Type { get; set; }
}