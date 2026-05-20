using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================
            // DATABASE
            // =========================================
            builder.Services.AddDbContext<SafexchangeDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // =========================================
            // AUTHENTICATION
            // =========================================
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme =
                        CookieAuthenticationDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        GoogleDefaults.AuthenticationScheme;
                })

                // COOKIE LOGIN
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";

                    options.AccessDeniedPath = "/AccessDenied";

                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                })

                // GOOGLE LOGIN
                .AddGoogle(options =>
                {
                    options.ClientId =
                        builder.Configuration["Authentication:Google:ClientId"];

                    options.ClientSecret =
                        builder.Configuration["Authentication:Google:ClientSecret"];
                });

            // =========================================
            // AUTHORIZATION
            // =========================================
            builder.Services.AddAuthorization();

            // =========================================
            // RAZOR PAGES
            // =========================================
            builder.Services.AddRazorPages();

            // =========================================
            // SESSION
            // =========================================
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);

                options.Cookie.HttpOnly = true;

                options.Cookie.IsEssential = true;
            });

            // =========================================
            // HTTP CONTEXT
            // =========================================
            builder.Services.AddHttpContextAccessor();

            // =========================================
            // CUSTOM SERVICES
            // =========================================
            builder.Services.AddScoped<ICartService, CartService>();

            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            var app = builder.Build();

            // =========================================
            // ERROR HANDLER
            // =========================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                app.UseHsts();
            }

            // =========================================
            // HTTPS
            // =========================================
            app.UseHttpsRedirection();

            // =========================================
            // STATIC FILES
            // =========================================
            app.UseStaticFiles();

            // =========================================
            // ROUTING
            // =========================================
            app.UseRouting();

            // =========================================
            // AUTHENTICATION
            // =========================================
            app.UseAuthentication();

            // =========================================
            // AUTHORIZATION
            // =========================================
            app.UseAuthorization();

            // =========================================
            // SESSION
            // =========================================
            app.UseSession();

            // =========================================
            // DEFAULT ROUTE
            // =========================================
            app.MapGet("/", context =>
            {
                if (context.User.Identity != null &&
                    context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Index");
                }
                else
                {
                    context.Response.Redirect("/Login");
                }

                return Task.CompletedTask;
            });

            // =========================================
            // RAZOR PAGES
            // =========================================
            app.MapRazorPages();

            app.Run();
        }
    }
}