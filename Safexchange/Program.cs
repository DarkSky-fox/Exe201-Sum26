using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DB Context
            builder.Services.AddDbContext<SafexchangeDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("ThuvienDB")));

            // Authentication
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme =
                        CookieAuthenticationDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId =
                        builder.Configuration["Authentication:Google:ClientId"];

                    options.ClientSecret =
                        builder.Configuration["Authentication:Google:ClientSecret"];
                });

            // Razor Pages
            builder.Services.AddRazorPages();

            // Session
            builder.Services.AddSession();

            var app = builder.Build();

            // Error Handler
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            // Authentication phải trước Authorization
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            app.MapRazorPages();

            app.Run();
        }
    }
}