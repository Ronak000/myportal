using Microsoft.AspNetCore.Mvc;
using MyPortal.Models;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.OData.Buffers;
using NAV;
using MyPortal.Services;

using System.Text.Json;
using MyPortal.DTO;
using System.Xml.Linq;
using Microsoft.OData.Edm;

namespace MyPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {

        private readonly UserServices _userService;
        public readonly AccountServices _accountServices;

        public AccountController(UserServices userService, AccountServices accountServices)
        {
            _accountServices = accountServices;
            _userService = userService;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Get()
        {
            return Ok(new { message = "Hello, Swagger!" });
        }
        /// <summary>
        /// Log in the user in the System
        /// </summary>
        /// <remarks>Login with email and password</remarks>
        /// <returns>User details if success, error message otherwise.</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return new BadRequestObjectResult("Enter email and password");
            }
            return await _userService.LoginUser(loginDTO.Email, loginDTO.Password);
        }

        /// <summary>
        /// generate temp password and send to user login email 
        /// </summary>
        /// <remarks>provide your email id</remarks>
        /// <returns>message return if success, error message otherwise.</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgotPassword forgotPassword)
        {
            if (string.IsNullOrEmpty(forgotPassword.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }
            else
            {
                return await _accountServices.TemporaryPassword(forgotPassword.Email);
            }
        }
        /// <summary>
        /// change your temp password to permanent password
        /// </summary>
        /// <remarks>enter new and confirm password</remarks>
        /// <returns>message return if success, error message otherwise.</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            if (string.IsNullOrEmpty(changePassword.Email) || string.IsNullOrEmpty(changePassword.Password))
            {
                return BadRequest(new { success = false, message = "Email and password is required" });
            }
            else
            {
                return await _accountServices.ChangePassword(changePassword.Email, changePassword.Password, changePassword.Type);
            }
        }
        /// <summary>
        /// List of orders of customer/vendor
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>Orders details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Orders(string No, string Type)
        {
            if (No != "null" && !string.IsNullOrEmpty(No))
            {
                if (Type == "Customer")
                {
                    return await _userService.GetCustomerOrders(No);
                }
                else
                {
                    return await _userService.GetVendorOrders(No);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// List of invoices of customer/vendor
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>Invoices details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Invoices(string No, string Type)
        {
            if (No != "null" && !string.IsNullOrEmpty(No))
            {
                if (Type == "Customer")
                {
                    return await _userService.GetCustomerInvoices(No);
                }
                else
                {
                    return await _userService.GetVendorInvoices(No);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// get earliest payment date
        /// </summary>
        /// <remarks>Enter No </remarks>
        /// <returns>date return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EarliestPaymentDate(string No)
        {
            if (!string.IsNullOrEmpty(No))
            {
                return await _userService.GetEarliestPaymentDate(No);
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// get earliest payment amount
        /// </summary>
        /// <remarks>Enter No </remarks>
        /// <returns>amount return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EarliestPaymentAmount(string No)
        {
            if (!string.IsNullOrEmpty(No))
            {
                return await _userService.GetEarliestPaymentAmount(No);
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// downlaod order report
        /// </summary>
        /// <remarks>enter unique no</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]/{OrderNo}")]
        public async Task<IActionResult> DownloadOrdersReports(string OrderNo, string Type)
        {
            if (Type == "Customer")
            {
                return await _userService.DownloadSalesOrdersReport(OrderNo);
            }
            else
            {
                return await _userService.DownloadPurchadeOrdersReport(OrderNo);
            }
        }
        /// <summary>
        /// downlaod invoice report
        /// </summary>
        /// <remarks>enter unique no</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]/{InvoiceNo}")]
        public async Task<IActionResult> DownloadInvoicesReport(string InvoiceNo, string Type)
        {
            if (Type == "Customer")
            {
                return await _userService.DownloadSalesInvoicesReport(InvoiceNo);
            }
            else
            {
                return await _userService.DownloadPurchaseInvoicesReport(InvoiceNo);
            }
        }
        /// <summary>
        /// downlaod statement report of customer
        /// </summary>
        /// <remarks>enter no, start date and end date</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpPost]
        [Route("[action]/{No}")]
        public async Task<IActionResult> DownloadCustomerStatements(string No, DateRange DateRange)
        {
            return await _userService.DownloadStatements(No, DateRange);
        }
        /// <summary>
        /// downlaod vedndor details
        /// </summary>
        /// <remarks>enter no</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]/{No}")]
        public async Task<IActionResult> DownloadVendorDetails(string No)
        {
            return await _userService.DownloadVendorDetails(No);
        }
        /// <summary>
        /// List of quotes of customer/vendor
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>Quotes details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Quotes(string No, string Type)
        {
            if (No != "null" && !string.IsNullOrEmpty(No))
            {
                if (Type == "Customer")
                {
                    return await _userService.GetCustomerQuotes(No);
                }
                else
                {
                    return await _userService.GetVendorQuotes(No);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// downlaod quote report
        /// </summary>
        /// <remarks>enter unique no</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]/{QuoteNo}")]
        public async Task<IActionResult> DownloadQuotesReport(string QuoteNo, string Type)
        {
            if (Type == "Customer")
            {
                return await _userService.DownloadSalesQuotesReport(QuoteNo);
            }
            else
            {
                return await _userService.DownloadPurchaseQuotesReport(QuoteNo);
            }
        }
        /// <summary>
        /// List of credit memo of customer/vendor
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>Credit memo details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CreditMemo(string No, string Type)
        {
            if (No != "null" && !string.IsNullOrEmpty(No))
            {
                if (Type == "Customer")
                {
                    return await _userService.GetCustomerSalesCreditMemo(No);
                }
                else
                {
                    return await _userService.GetVendorSalesCreditMemo(No);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// downlaod sales credit memo report
        /// </summary>
        /// <remarks>enter unique no</remarks>
        /// <returns>string return if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]/{SalesCreditMemoNo}")]
        public async Task<IActionResult> DownloadCreditMemoReport(string SalesCreditMemoNo, string Type)
        {
            if (Type == "Customer")
            {
                return await _userService.DownloadSalesCreditMemoReport(SalesCreditMemoNo);
            }
            else
            {
                return await _userService.DownloadPurchaseCreditMemoReport(SalesCreditMemoNo);
            }
        }
        /// <summary>
        /// second login api for both type
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>USer details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> TypeBaseLogin(string No, string Type)
        {
            if (No != "null" && !string.IsNullOrEmpty(No))
            {
                if (Type == "Customer")
                {
                    var UseDetails = await _userService.GetCustomerDetails(No);
                    return new OkObjectResult(UseDetails);
                }
                else
                {
                    var VendorDetails = await _userService.GetVendorDetails(No);
                    return new OkObjectResult(VendorDetails);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        /// <summary>
        /// send temp password based on type
        /// </summary>
        /// <remarks>Enter No and type</remarks>
        /// <returns>USer details if success, error message otherwise.</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> TypeBaseForgetPassword(string Type, string Email)
        {
            if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Type))
            {
                return BadRequest(new { success = false, message = "Email and Type is required" });
            }
            else
            {
                string urlDecodedUsername = Uri.UnescapeDataString(Email);
                string decodedEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(urlDecodedUsername));
                return await _userService.GetTempPassword(Type, decodedEmail);
            }
        }
    }
}
public class ForgotPassword
{
    public string Email { get; set; }
}
public class ChangePassword
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Type { get; set; }
}
public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
