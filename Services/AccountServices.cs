using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.DTO;
using MyPortal.Models;

namespace MyPortal.Services
{

    public class AccountServices
    {
        private readonly UserServices _userService;
        public AccountServices(UserServices userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> TemporaryPassword(string Email)
        {
           return await _userService.CheckUSerExist(Email);   
        }

        public async Task<IActionResult> ChangePassword(string email, string password, string type)
        {
            var Temp = await _userService.GetChangePassword(email, password, type);
            if (!Temp)
            {
                return new BadRequestObjectResult(new { success = false, message = "Error generating temporary password or email not found." });
            }
            else
            {
                return new OkObjectResult(new { success = true, message = "A password has been changed." });
            }
        }
    }
}