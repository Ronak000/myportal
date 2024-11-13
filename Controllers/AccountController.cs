using Microsoft.AspNetCore.Mvc;
using MyPortal.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.OData.Buffers;
using Microsoft.Identity.Client;
using NAV;
using MyPortal.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MyPortal.DTO;
using MyPortal.Data;
using System.Xml.Linq;

namespace MyPortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private readonly UserServices _userService;
        public readonly AccountServices _accountServices;
        protected readonly DatabaseContext _context;

        public AccountController(UserServices userService, AccountServices accountServices, DatabaseContext context)
        {
            _accountServices = accountServices;
            _userService = userService;
            _context = context;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Get()
        {
            return Ok(new { message = "Hello, Swagger!" });
        }
        /// <summary>
        /// Logging the user in the System
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
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddUser([FromBody] UserDetailsDTO userDetailsDTO)
        {
            if (userDetailsDTO == null)
            {
                return new BadRequestObjectResult("Enter user details");
            }
            return await _accountServices.AddUSer(userDetailsDTO);
        }
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
                return await _accountServices.ChangePassword(changePassword.Email, changePassword.Password);
            }
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Orders(string No)
        {
            if (!string.IsNullOrEmpty(No))
            {
                return await _userService.GetOrders(No);
            }
            else
            {
                return new BadRequestResult();
            }
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Invoices(string No)
        {
            if (!string.IsNullOrEmpty(No))
            {
                return await _userService.GetInvoices(No);
            }
            else
            {
                return new BadRequestResult();
            }
        }
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
        [HttpGet]
        [Route("[action]/{OrderNo}")]
        public async Task<IActionResult> DownloadOrders(string OrderNo)
        {
            return await _userService.DownloadOrders(OrderNo);
        }
        [HttpGet]
        [Route("[action]/{InvoiceNo}")]
        public async Task<IActionResult> DownloadInvoices(string InvoiceNo)
        {
            return await _userService.DownloadInvoices(InvoiceNo);
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
}
