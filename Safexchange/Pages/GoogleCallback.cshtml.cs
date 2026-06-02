using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
            return RedirectToPage("/Login", new { message = "Đăng nhập Google thất bại. Vui lòng thử lại." });
        }

        var email = googleResult.Principal.FindFirstValue(ClaimTypes.Email)
            ?? googleResult.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Login", new { message = "Tài khoản Google không có email." });
        }

        email = email.Trim().ToLowerInvariant();

        var fullName = googleResult.Principal.FindFirstValue(ClaimTypes.Name)
            ?? googleResult.Principal.FindFirstValue("name")
            ?? email.Split('@')[0];

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                FullName = fullName,
                PasswordHash = PasswordHasher.HashGooglePlaceholder(email),
                Role = "student",
                AccountStatus = "active",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
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

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        _currentUser.SetUserId(user.UserId);

        if (string.Equals(user.Role, "shipper", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Shipper/Index");
        }

        return RedirectToPage("/Index");
    }
}
