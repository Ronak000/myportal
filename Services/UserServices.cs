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
using MyPortal.Services.ReportService;

namespace MyPortal.Services
{ 
    public class UserServices
    {
        private static List<ClientUser> cachedUsers;
        private static string accessToken;
        private static DateTime tokenExpiry;

        public IConfiguration _configuration { get; }
        public IHttpContextAccessor _context { get; }
        protected readonly ICustomerService _customerService;

        protected readonly IVendorService _vendorService;
        public IReportServices _reportServices { get; }
        private readonly ITokenManager _tokenManager;

        public UserServices(IConfiguration configuration, IHttpContextAccessor context, ICustomerService customerService, IVendorService vendorService, IReportServices reportServices, ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
            _reportServices = reportServices;
            _vendorService = vendorService;
            _customerService = customerService;
            _configuration = configuration;
            _context = context;
        }

        public async Task InitializeAsync()
        {

            // Get the access token
            accessToken = await GetAccessTokenAsync();

        }
        public async Task<string> GetAccessTokenAsync()
        {
            var MicrosoftUrl = _configuration.GetSection("AzureAD").GetValue<string>("Instance");
            var TenantId = _configuration.GetSection("AzureAD").GetValue<string>("TenantId");
            var ClientId = _configuration.GetSection("AzureAD").GetValue<string>("ClientId");
            var ClientSecret = _configuration.GetSection("AzureAd").GetValue<string>("ClientSecret");
            if (string.IsNullOrEmpty(accessToken) || tokenExpiry <= DateTime.UtcNow)
            {
                // Get a new access token
                var newTokenResponse = await _tokenManager.GetNewAccessTokenAsync(MicrosoftUrl, TenantId, ClientId, ClientSecret);
                accessToken = newTokenResponse.access_token;
                tokenExpiry = DateTime.UtcNow.AddSeconds(newTokenResponse.expires_in - 60); // Token expiry buffer
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
                ODataResponse<ClientUser>? users = new ODataResponse<ClientUser>();
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<ODataResponse<ClientUser>>(data);
                    return users.Value;
                }
                else
                {
                    Console.WriteLine("Failed to get data: " + response.StatusCode);
                    return users.Value;
                }
            }
        }

        public async Task<IActionResult> LoginUser(string Email, string password)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            cachedUsers = await FetchUsersFromService(accessToken, Email);
            if(cachedUsers == null)
            {
                return new BadRequestObjectResult("User Data not found!");
            }
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
                            return new OkObjectResult(new { redirected = true, UsersData = Users });
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
        
        public async Task<IActionResult> CheckUSerExist(string email)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            cachedUsers = await FetchUsersFromService(accessToken, email);
            if(cachedUsers == null)
            {
                return new BadRequestObjectResult("User Data not found!");
            }
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
        // Customer Data
        public async Task<Dictionary<string, Object>> GetCustomerDetails(string UniqueNo)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _customerService.CustomerDetails(UniqueNo, accessToken);
        }
        public async Task<IActionResult> GetCustomerOrders(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _customerService.CustomerOrders(No, accessToken);
        }
        public async Task<IActionResult> GetCustomerInvoices(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _customerService.CustomerInvoices(No, accessToken);
        }
        public async Task<IActionResult> GetCustomerQuotes(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _customerService.CustomerQuotes(No, accessToken);
        }
        public async Task<IActionResult> GetCustomerSalesCreditMemo(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _customerService.CustomerSalesCreditMemo(No, accessToken);
        }
        public async Task<IActionResult> GetEarliestPaymentDate(string No)
        {
            return await _customerService.GetEarliestPaymentDate(No, accessToken);
        }
        public async Task<IActionResult> GetEarliestPaymentAmount(string No)
        {
            return await _customerService.GetEarliestPaymentAmount(No, accessToken);
        }

        // Customers all different types of reports
        public async Task<IActionResult> DownloadStatements(string No, DateRange DateRange)
        {
            return await _reportServices.customerReportServices().DownloadCustomerStatementReport(No, DateRange, accessToken);
        }
        public async Task<IActionResult> DownloadSalesOrdersReport(string OrderNo)
        {
            return await _reportServices.customerReportServices().DownloadSalesOrderReport(OrderNo, accessToken);
        }
        public async Task<IActionResult> DownloadSalesInvoicesReport(string InvoiceNo)
        {
            return await _reportServices.customerReportServices().DownloadSalesInvoicesReport(InvoiceNo, accessToken);
        }
        public async Task<IActionResult> DownloadSalesQuotesReport(string QuoteNo)
        {
            return await _reportServices.customerReportServices().DownloadSalesQuotesReport(QuoteNo, accessToken);
        }
        public async Task<IActionResult> DownloadSalesCreditMemoReport(string SalesCreditMemoNo)
        {
            return await _reportServices.customerReportServices().DownloadSalesCreditMemoReport(SalesCreditMemoNo, accessToken);
        }

        // Vendor Data
        public async Task<Dictionary<string, Object>> GetVendorDetails(string UniqueNo)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _vendorService.VendorDetails(UniqueNo, accessToken);
        }
        
        public async Task<IActionResult> GetVendorOrders(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _vendorService.VendorOrders(No, accessToken);
        }
        
        public async Task<IActionResult> GetVendorInvoices(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _vendorService.VendorInvoices(No, accessToken);
        }
        public async Task<IActionResult> GetVendorQuotes(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _vendorService.VendorQuotes(No, accessToken);
        }
        public async Task<IActionResult> GetVendorSalesCreditMemo(string No)
        {
            if (tokenExpiry <= DateTime.UtcNow)
            {
                accessToken = await GetAccessTokenAsync();
            }
            return await _vendorService.VendorSalesCreditMemo(No, accessToken);
        }

        // Vendor's all different types of reports
        public async Task<IActionResult> DownloadVendorDetails(string No)
        {
            return await _reportServices.vendorReportServices().DownloadVendorDetails(No, accessToken);
        }
        public async Task<IActionResult> DownloadPurchadeOrdersReport(string OrderNo)
        {
            return await _reportServices.vendorReportServices().DownloadVendorOrderReport(OrderNo, accessToken);
        }
        public async Task<IActionResult> DownloadPurchaseInvoicesReport(string InvoiceNo)
        {
            return await _reportServices.vendorReportServices().DownloadVendorInvoiceReport(InvoiceNo, accessToken);
        }
        public async Task<IActionResult> DownloadPurchaseQuotesReport(string QuoteNo)
        {
            return await _reportServices.vendorReportServices().DownloadVendorQuoteReport(QuoteNo, accessToken);
        }     
        public async Task<IActionResult> DownloadPurchaseCreditMemoReport(string SalesCreditMemoNo)
        {
            return await _reportServices.vendorReportServices().DownloadVendorCreditMemoReport(SalesCreditMemoNo, accessToken);
        }
        
    }
}