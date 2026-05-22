using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using System.Text;
using System.Security.Cryptography;

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

        public IActionResult OnPost()
        {
            string hashedPassword = HashPassword(Password);

            var user = _context.Users.FirstOrDefault(x =>
                x.Email == Email &&
                x.PasswordHash == hashedPassword);

            if (user == null)
            {
                Message = "Invalid Email or Password!";
                return Page();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);

            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToPage("/Index");
        }

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
