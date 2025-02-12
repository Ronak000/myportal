using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyPortal.Filters
{
    public class ApiKeyFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;
        public ApiKeyFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path.ToString();

            // Only apply API key/password validation for API requests (URLs containing /api/)
            if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                return; // Skip validation for non-API requests
            }
            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var apiKey) ||
                !context.HttpContext.Request.Headers.TryGetValue("X-API-PASSWORD", out var apiPassword))
            {
                context.Result = new UnauthorizedObjectResult("API Key and Password are required.");
                return;
            }

            string expectedApiKey = _configuration.GetSection("ApiSettings").GetValue<string>("ApiKey");
            string expectedApiPassword = _configuration.GetSection("ApiSettings").GetValue<string>("ApiPassword");

            if (!expectedApiKey.Equals(apiKey) || !expectedApiPassword.Equals(apiPassword))
            {
                context.Result = new UnauthorizedObjectResult("Invalid API Key or Password.");
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}