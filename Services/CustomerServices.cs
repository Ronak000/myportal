using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services
{
    public class CustomerServices : ICustomerService
    {
        private readonly IConfiguration _configuration;
        public CustomerServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Dictionary<string,Object>> CustomerDetails(string UniqueNo, string accessToken)
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
                    var customerObject = JsonSerializer.Deserialize<Dictionary<string, object>>(Customer[0].GetRawText());
                    return customerObject;
                }
                else
                {
                    Console.WriteLine("Failed to retrieve user details one: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> CustomerOrders(string No, string accessToken)
        {
            string CustomerOrderUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesOrder");
            string FilterUrl = $"{CustomerOrderUrl}/?$filter=sellToCustomerNo eq '{No}'";
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

        public async Task<IActionResult> CustomerInvoices(string No, string accessToken)
        {
            string CustomerInvoiceUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesInvoice");
            string FilterUrl = $"{CustomerInvoiceUrl}/?$filter=sellToCustomerNo eq '{No}'";
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

        public async Task<IActionResult> CustomerQuotes(string No, string accessToken)
        {
            string CustomerQuateUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesQuotes");
            string FilterUrl = $"{CustomerQuateUrl}/?$filter=sellToCustomerNo eq '{No}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var Orders = JsonDocument.Parse(ResponseData);
                    var QuatesData = Orders.RootElement.GetProperty("value");
                    return new OkObjectResult(QuatesData);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve orders: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> CustomerSalesCreditMemo(string No, string accessToken)
        {
            string CustomerSalesCreditMemoUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesCreditMemo");
            string FilterUrl = $"{CustomerSalesCreditMemoUrl}/?$filter=sellToCustomerNo eq '{No}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var Orders = JsonDocument.Parse(ResponseData);
                    var SalesCreditMemoData = Orders.RootElement.GetProperty("value");
                    return new OkObjectResult(SalesCreditMemoData);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve orders: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> GetEarliestPaymentDate(string No, string accessToken)
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

        public async Task<IActionResult> GetEarliestPaymentAmount(string No, string accessToken)
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
    }
}