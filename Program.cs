
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using MyPortal;
using MyPortal.Data;
using MyPortal.Services;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyPortal.Middlewares;
using MyPortal.Filters;
using MyPortal.Services.ReportService;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var Configuration = builder.Configuration;
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Console.WriteLine($"Current Environment: {env}");
        builder.Services.AddAuthentication();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllersWithViews();
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiKeyFilter>(); // Add the filter globally
        });
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IConfiguration>(Configuration);
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddScoped<UserServices>();
        builder.Services.AddScoped<TokenManager>();
        builder.Services.AddScoped<AccountServices>();
        builder.Services.AddScoped<CustomerServices>();
        builder.Services.AddScoped<VendorServices>();
        builder.Services.AddScoped<ICustomerService, CustomerServices>();
        builder.Services.AddScoped<IVendorService, VendorServices>();
        builder.Services.AddScoped<IReportServices, ReportService>();
        builder.Services.AddScoped<ICustomerReportServices, CustomerReportServices>();
        builder.Services.AddScoped<IVendorReportServices, VendorReportServices>();


        // sql server connection string
        // builder.Services.AddDbContext<DatabaseContext>(option =>
        //     option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add services to the container.
        var Serverlocal = new OpenApiServer();
        Serverlocal.Url = "https://localhost:7003";

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "My Portal",
                Version = "v1"
            });
            options.OperationFilter<AddCustomHeaderOperationFilter>();
            options.AddServer(Serverlocal);
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }
        );



        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

            // Call methods to fetch tokens or initialize
            userService.InitializeAsync(); // Assuming you have these methods in the service classes

        }
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHsts();
        app.UseCors(
                options => options.WithOrigins()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition")
            );
        app.UseSwagger();
        app.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            option.RoutePrefix = "swagger";
        });
        // app.UseMiddleware<ApiKeyMiddleware>();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();

        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        //app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}");

        app.Run();
    }
}