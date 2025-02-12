using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Models;

namespace MyPortal.Controllers;

public class HomeController : Controller
{
    

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Customer_Home()
    {
        return View();
    }
    public IActionResult Orders()
    {
        return View();
    }
    public IActionResult Invoices()
    {
        return View();
    }
    public IActionResult Quotes()
    {
        return View();
    }
    public IActionResult Credit_Memo()
    {
        return View();
    }
    public IActionResult Change_Password(string email)
    {
        try
        {
            string urlDecodedUsername = Uri.UnescapeDataString(email);
            string decodedEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(urlDecodedUsername));

            ViewBag.Email = decodedEmail;
            return View();
        }
        catch (FormatException)
        {
            return BadRequest("Invalid email format.");
        }
    }
    public IActionResult Logout()
    {
        return new NoContentResult();
    }
}
