using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;
using System.Security.Cryptography;
using System.Text;

namespace Safexchange.Pages
{
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

            _currentUser.SignIn(user.UserId, user.Email, user.Role);

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
