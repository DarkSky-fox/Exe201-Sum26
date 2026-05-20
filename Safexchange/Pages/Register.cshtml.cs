using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using System.Security.Cryptography;
using System.Text;

namespace Safexchange.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly SafexchangeDbContext _context;

        public RegisterModel(SafexchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User Input { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Message { get; set; }

        public IActionResult OnPost()
        {
            var existingUser = _context.Users
                .FirstOrDefault(x => x.Email == Input.Email);

            if (existingUser != null)
            {
                Message = "Email already exists!";
                return Page();
            }

            Input.PasswordHash = HashPassword(Password);

            Input.Role = "student";

            Input.AccountStatus = "active";

            Input.CreatedAt = DateTime.Now;

            _context.Users.Add(Input);

            _context.SaveChanges();

            return RedirectToPage("/Login");
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
