using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services.ReportService
{
    public class CustomerReportServices : ICustomerReportServices
    {
        // public IConfiguration _configuration { get; }
        public IConfiguration _configuration { get; set; }
        public CustomerReportServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> DownloadSalesOrderReport(string OrderNo, string accessToken)
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
                    Console.WriteLine("Error while downloading sales order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadSalesInvoicesReport(string InvoiceNo, string accessToken)
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
                    Console.WriteLine("Error while downloading sales invoice report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadSalesQuotesReport(string QuoteNo, string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadSalesQuoteReport";
            var SoapXml = $@"
           <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadSalesQuoteReport>
                        <urn:salesQuoteNo_iCod>{QuoteNo}</urn:salesQuoteNo_iCod>
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
                    Console.WriteLine("Error while downloading sales quotes report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadSalesCreditMemoReport(string SalesCreditMemoNo, string accessToken)
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
                    Console.WriteLine("Error while downloading sales creditmemo report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadCustomerStatementReport(string No, DateRange DateRange, string accessToken)
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
                    Console.WriteLine("Error while downloading customer statement report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }
    }
}