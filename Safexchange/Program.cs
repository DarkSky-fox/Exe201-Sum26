using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Safexchange.Hubs;
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
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                })
                .AddGoogle(options =>
                {
                    options.ClientId =
                        builder.Configuration["Authentication:Google:ClientId"]
                        ?? string.Empty;

                    options.ClientSecret =
                        builder.Configuration["Authentication:Google:ClientSecret"]
                        ?? string.Empty;

                    options.CallbackPath = "/signin-google";
                    options.SaveTokens = true;
                });

            // =========================================
            // SIGNALR
            // =========================================
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            });

            builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

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
            builder.Services.AddScoped<ICartAddService, CartAddService>();

            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddScoped<ICheckoutService, CheckoutService>();

            builder.Services.AddScoped<IShipmentService, ShipmentService>();

            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddSingleton<IProductImageStorage, ProductImageStorage>();

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

            var assetsPath = Path.Combine(app.Environment.ContentRootPath, "Assets");
            Directory.CreateDirectory(assetsPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(assetsPath),
                RequestPath = "/Assets"
            });

            // =========================================
            // ROUTING
            // =========================================
            app.UseRouting();

            app.UseHttpsRedirection();
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
           

            // =========================================
            // SIGNALR HUB ROUTING
            // =========================================
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<NotificationHub>("/hubs/notification");

            // =========================================
            // RAZOR PAGES
            // =========================================
            app.MapRazorPages();

            app.Run();
        }
    }
}