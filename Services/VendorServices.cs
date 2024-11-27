using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services
{
    public class VendorServices
    {
        private readonly IConfiguration _configuration;
        public VendorServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Dictionary<string,Object>> VendorDetails(string UniqueNo, string accessToken)
        {
            string VendorDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("Vendor_DetailsUrl");
            string FilterUrl = $"{VendorDetailsUrl}/?$filter=No eq '{UniqueNo}'";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(FilterUrl);

                if (response.IsSuccessStatusCode)
                {
                    var ResponseData = await response.Content.ReadAsStringAsync();
                    var VendorData = JsonDocument.Parse(ResponseData);
                    var Vendor = VendorData.RootElement.GetProperty("value");
                    var VendorObject = JsonSerializer.Deserialize<Dictionary<string, object>>(Vendor[0].GetRawText());
                    return VendorObject;
                }
                else
                {
                    Console.WriteLine("Failed to retrieve user details one: " + response.StatusCode);
                    return null;
                }
            }
        }

        public async Task<IActionResult> VendorOrders(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("PurchaseOrder");
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

        public async Task<IActionResult> VendorInvoices(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("PurchaseInvoice");
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

        public async Task<IActionResult> VendorQuates(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("PurchaseQuote");
            string FilterUrl = $"{CustomerDetailsUrl}/?$filter=sellToCustomerNo eq '{No}'";
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

        public async Task<IActionResult> VendorSalesCreditMemo(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("PurchaseCreditMemo");
            string FilterUrl = $"{CustomerDetailsUrl}/?$filter=sellToCustomerNo eq '{No}'";
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
    }
}