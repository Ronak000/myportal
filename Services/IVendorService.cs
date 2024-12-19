using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services
{
    public interface IVendorService
    {
        Task<Dictionary<string,Object>> VendorDetails(string UniqueNo, string accessToken);
        Task<IActionResult> VendorOrders(string No, string accessToken);
        Task<IActionResult> VendorInvoices(string No, string accessToken);
        Task<IActionResult> VendorQuotes(string No, string accessToken);
        Task<IActionResult> VendorSalesCreditMemo(string No, string accessToken);
    }
}