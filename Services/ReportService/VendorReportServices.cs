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
    public class VendorReportServices : IVendorReportServices
    {
        public IConfiguration _configuration { get; set; }
        public VendorReportServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> DownloadVendorOrderReport(string OrderNo, string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadPurchaseOrderReport";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadPurchaseOrderReport>
                        <urn:purchaseOrderNo_iCod>{OrderNo}</urn:purchaseOrderNo_iCod>
                    </urn:DownloadPurchaseOrderReport>
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
                    Console.WriteLine("Error while downloading purchase order report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadVendorInvoiceReport(string InvoiceNo, string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadPurchaseInvoiceReport";
            var SoapXml = $@"
            <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                        <soapenv:Header/>
                        <soapenv:Body>
                            <urn:DownloadPurchaseInvoiceReport>
                                <urn:purchaseInvoiceNo_iCod>{InvoiceNo}</urn:purchaseInvoiceNo_iCod>
                            </urn:DownloadPurchaseInvoiceReport>
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
                    Console.WriteLine("Error while downloading purchase invoice report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadVendorQuoteReport(string QuoteNo, string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadPurchaseQuoteReport";
            var SoapXml = $@"
           <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadPurchaseQuoteReport>
                        <urn:purchaseQuoteNo_iCod>{QuoteNo}</urn:purchaseQuoteNo_iCod>
                    </urn:DownloadPurchaseQuoteReport>
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
                    Console.WriteLine("Error while downloading purchase quotes report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadVendorCreditMemoReport(string SalesCreditMemoNo, string accessToken)
        {
            var BCCustomerLoginApiUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("ChangePassword");
            string SOAPAction = "urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS:DownloadPurchasesCreditMemoReport";
            var SoapXml = $@"
           <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:microsoft-dynamics-schemas/codeunit/CP_Functionality_WS'>
                <soapenv:Header/>
                <soapenv:Body>
                    <urn:DownloadPurchasesCreditMemoReport>
                        <urn:purchaseCreditMemoNo_iCod>{SalesCreditMemoNo}</urn:purchaseCreditMemoNo_iCod>
                    </urn:DownloadPurchasesCreditMemoReport>
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
                    Console.WriteLine("Error while downloading purchase creditmemo report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }

        public async Task<IActionResult> DownloadVendorDetails(string No, string accessToken)
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
                    Console.WriteLine("Error while downloading vendor ledger report:" + Response.IsSuccessStatusCode);
                    return new BadRequestObjectResult(Response.IsSuccessStatusCode);
                }
            }
        }
    }
}