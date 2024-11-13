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

namespace MyPortal.Services
{
    public class UserServices
    {
        private static List<ClientUser> cachedUsers;
        private static string accessToken;

        public IConfiguration _configuration { get; }
        public IHttpContextAccessor _context { get; }

        public UserServices(IConfiguration configuration, IHttpContextAccessor context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var tenantId = _configuration.GetSection("AzureAD").GetValue<string>("TenantId");
            var clientId = _configuration.GetSection("AzureAD").GetValue<string>("ClientId");
            var clientSecret = _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret");
            // Get the access token
            accessToken = await TokenManager.GetAccessTokenAsync(tenantId, clientId, clientSecret);

        }

        private async Task<List<ClientUser>> FetchUsersFromService(string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("CustomerLoginUrl");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(BCCustomerLoginApiUrl);

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

            cachedUsers = await FetchUsersFromService(accessToken);

            var user = cachedUsers.Where(u => u.Email == Email && u.Password == password).FirstOrDefault();
            if (user != null)
            {
                if (user.Temporary == false)
                {
                    var UserDetails = await GetUserDetailsFromWebService(user.No.ToString());

                    if (UserDetails != null)
                    {
                        return new OkObjectResult(UserDetails);
                    }
                    else
                    {
                        return new BadRequestObjectResult("Failed to retrieve user details");
                    }
                }
                else
                {
                    byte[] emailBytes = Encoding.UTF8.GetBytes(Email);
                    string email = Convert.ToBase64String(emailBytes);
                    return new RedirectToActionResult("Change_Password", "Home", new { email });
                }
            }
            else
            {
                return new UnauthorizedObjectResult("Invalid username or password");
            }
        }
        private async Task<IActionResult> GetUserDetailsFromWebService(string UniqueNo)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("CustomerDetailsUrl");
            string FilterUrl = $"{CustomerDetailsUrl}/?$filter=No eq '{UniqueNo}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var CustomerData = JsonDocument.Parse(ResponseData);
                    var Customer = CustomerData.RootElement.GetProperty("value");
                    return new OkObjectResult(Customer);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve user details one: " + response.StatusCode);
                    return null;
                }
            }
        }
        public async Task<bool> CheckUSerExist(string email)
        {
            cachedUsers = await FetchUsersFromService(accessToken);
            var IsUserExist = cachedUsers.Any(x => x.Email == email);
            return IsUserExist;
        }

        public async Task<bool> GetTempPassword(string Email)
        {

            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:GenerateTempPassword";
            var SoapXml = $@"
                    <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                    <soapenv:Header/>
                    <soapenv:Body>
                        <urn:GenerateTempPassword>
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
                    var IsSent = bool.Parse(RootElement.Value);
                    return IsSent;
                }
                else
                {
                    Console.WriteLine("Error while sending temporary password:" + Response.IsSuccessStatusCode);
                    return false;
                }
            }

        }

        public async Task<bool> GetChangePassword(string email, string password)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:ChangePassword";
            var SoapXml = $@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' 
                            xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                                <soapenv:Header/>
                                <soapenv:Body>
                                    <urn:ChangePassword>
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
        public async Task<IActionResult> GetOrders(string No)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesOrder");
            string FilterUrl = $"{CustomerDetailsUrl}/?$filter=sellToCustomerNo eq '{No}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var Orders = JsonDocument.Parse(ResponseData);
                    var OrdersData = Orders.RootElement.GetProperty("value");
                    return new OkObjectResult(OrdersData);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve orders: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> GetInvoices(string No)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesInvoice");
            string FilterUrl = $"{CustomerDetailsUrl}/?$filter=sellToCustomerNo eq '{No}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var Orders = JsonDocument.Parse(ResponseData);
                    var OrdersData = Orders.RootElement.GetProperty("value");
                    return new OkObjectResult(OrdersData);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve orders: " + response.StatusCode);
                    return null;
                }
            }
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
                    return new OkObjectResult(new { Date =Date});
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
                    var IsDownload = RootElement.Value;
                    return new OkObjectResult(new { base64 = IsDownload});
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
                    var IsDownload = RootElement.Value;
                    return new OkObjectResult(new { base64 = IsDownload});
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
