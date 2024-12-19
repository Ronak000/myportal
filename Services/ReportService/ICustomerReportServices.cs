using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.Services.ReportService
{
    public interface ICustomerReportServices
    {
        Task<IActionResult> DownloadSalesOrderReport(string OrderNo, string accessToken);
        Task<IActionResult> DownloadSalesInvoicesReport(string InvoiceNo, string accessToken);
        Task<IActionResult> DownloadSalesQuotesReport(string QuoteNo, string accessToken);
        Task<IActionResult> DownloadSalesCreditMemoReport(string SalesCreditMemoNo, string accessToken);
        Task<IActionResult> DownloadCustomerStatementReport(string No, DateRange DateRange, string accessToken);
    }
}