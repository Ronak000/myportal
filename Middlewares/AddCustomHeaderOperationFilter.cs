using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyPortal.Middlewares
{
    public class AddCustomHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the parameters list exists, if not, initialize it
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Add X-API-KEY Header
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Required = true, // Set to true if the header is required
                Description = "Enter your API Key here",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });

            // Add X-API-PASSWORD Header
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-PASSWORD",
                In = ParameterLocation.Header,
                Required = true, // Set to true if the header is required
                Description = "Enter your API Password here",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}