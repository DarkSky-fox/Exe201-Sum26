using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;
using System.Security.Claims;
namespace Safexchange.Pages;

public class LoginModel : PageModel
{
    private readonly SafexchangeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LoginModel(SafexchangeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [BindProperty]
    public LoginForm Input { get; set; } = new();

    public string? Message { get; set; }

    public void OnGet(string? message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Message = message;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim().ToLowerInvariant();
        var user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == email);

        if (user is null)
        {
            Message = "Email hoặc mật khẩu không đúng.";
            return Page();
        }

        if (PasswordHasher.IsGoogleAccount(email, user.PasswordHash))
        {
            Message = "Tài khoản này đăng ký bằng Google. Vui lòng dùng nút Continue with Google.";
            return Page();
        }

        if (user.PasswordHash != PasswordHasher.HashPassword(Input.Password))
        {
            Message = "Email hoặc mật khẩu không đúng.";
            return Page();
        }

        await SignInUserAsync(user);

        if (string.Equals(user.Role, "shipper", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Shipper/Index");
        }

        return RedirectToPage("/Index");
    }

    private async Task SignInUserAsync(User user)
    {
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
    }

    public class LoginForm
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
