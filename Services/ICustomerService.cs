using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services
{
    public interface ICustomerService
    {
        Task<Dictionary<string,Object>> CustomerDetails(string UniqueNo, string accessToken);
        Task<IActionResult> CustomerOrders(string No, string accessToken);
        Task<IActionResult> CustomerInvoices(string No, string accessToken);
        Task<IActionResult> CustomerQuotes(string No, string accessToken);
        Task<IActionResult> CustomerSalesCreditMemo(string No, string accessToken);
        Task<IActionResult> GetEarliestPaymentDate(string no, string accessToken);
        Task<IActionResult> GetEarliestPaymentAmount(string no, string accessToken);
    }
}