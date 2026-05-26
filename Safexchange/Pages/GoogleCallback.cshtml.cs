using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages;

public class GoogleCallbackModel : PageModel
{
    private readonly SafexchangeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GoogleCallbackModel(SafexchangeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var googleResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!googleResult.Succeeded || googleResult.Principal == null)
        {
            return RedirectToPage("/Login", new { message = "Google sign-in failed. Please try again." });
        }

        var email = googleResult.Principal.FindFirstValue(ClaimTypes.Email)
            ?? googleResult.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Login", new { message = "Google account has no email." });
        }

        var fullName = googleResult.Principal.FindFirstValue(ClaimTypes.Name)
            ?? googleResult.Principal.FindFirstValue("name")
            ?? email.Split('@')[0];

        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                FullName = fullName,
                PasswordHash = HashGooglePlaceholder(email),
                Role = "student",
                AccountStatus = "active",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("UserId", user.UserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        _currentUser.SetUserId(user.UserId);

        if (string.Equals(user.Role, "shipper", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Shipper/Index");
        }

        return RedirectToPage("/Index");
    }

    private static string HashGooglePlaceholder(string email)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("GOOGLE_OAUTH:" + email));
        return Convert.ToBase64String(bytes);
    }
}
