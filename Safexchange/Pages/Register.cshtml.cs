using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;
using Safexchange.Validation;
namespace Safexchange.Pages;

public class RegisterModel : PageModel
{
    private readonly SafexchangeDbContext _context;

    public RegisterModel(SafexchangeDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RegisterForm Input { get; set; } = new();

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim().ToLowerInvariant();
        var existingUser = _context.Users.FirstOrDefault(x => x.Email.ToLower() == email);
        if (existingUser is not null)
        {
            Message = "Email đã được sử dụng.";
            return Page();
        }

        var normalizedPhone = VietnamesePhoneAttribute.Normalize(Input.Phone);
        if (normalizedPhone is null)
        {
            ModelState.AddModelError("Input.Phone", "Số điện thoại không hợp lệ. Vui lòng nhập số Việt Nam (vd: 0912345678).");
            return Page();
        }

        var user = new User
        {
            FullName = Input.FullName.Trim(),
            Email = email,
            Phone = normalizedPhone,
            YearLevel = Input.YearLevel,
            PasswordHash = PasswordHasher.HashPassword(Password),
            Role = "student",
            AccountStatus = "active",
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToPage("/Login", new { message = "Đăng ký thành công. Vui lòng đăng nhập." });
    }

    public class RegisterForm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2–100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [VietnamesePhone]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn năm học.")]
        [Display(Name = "Năm học")]
        public string YearLevel { get; set; } = string.Empty;
    }
}
