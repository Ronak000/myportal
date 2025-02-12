using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services.ReportService
{
    public interface IVendorReportServices
    {
        Task<IActionResult> DownloadVendorCreditMemoReport(string SalesCreditMemoNo, string accessToken);
        Task<IActionResult> DownloadVendorDetails(string No, string accessToken);
        Task<IActionResult> DownloadVendorInvoiceReport(string InvoiceNo, string accessToken);
        Task<IActionResult> DownloadVendorOrderReport(string OrderNo, string accessToken);
        Task<IActionResult> DownloadVendorQuoteReport(string QuoteNo, string accessToken);
    }
}