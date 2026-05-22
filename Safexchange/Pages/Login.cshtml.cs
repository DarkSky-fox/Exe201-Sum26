using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Safexchange.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SafexchangeDbContext _context;

        public LoginModel(SafexchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // HASH PASSWORD
            string hashedPassword = HashPassword(Password);

            // CHECK USER
            var user = _context.Users.FirstOrDefault(x =>
                x.Email == Email &&
                x.PasswordHash == hashedPassword);

            if (user == null)
            {
                Message = "Invalid Email or Password!";
                return Page();
            }

            // CLAIMS
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            // IDENTITY
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // PRINCIPAL
            var principal = new ClaimsPrincipal(identity);

            // SIGN IN
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // REDIRECT
            return RedirectToPage("/Index");
        }

        // HASH PASSWORD
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(password));

                return Convert.ToBase64String(bytes);
            }
        }
    }
}