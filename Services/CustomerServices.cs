using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services
{
    public class CustomerServices
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

        public async Task<IActionResult> CustomerInvoices(string No, string accessToken)
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

        public async Task<IActionResult> CustomerQuates(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesQuates");
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

        public async Task<IActionResult> CustomerSalesCreditMemo(string No, string accessToken)
        {
            string CustomerDetailsUrl = _configuration.GetSection("BusinessCentralServices").GetValue<string>("SalesCreditMemoNo");
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