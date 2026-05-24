using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Seller
{
    public class VerifyAccountModel : PageModel
    {
        private readonly Safexchange.Models.SafexchangeDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public VerifyAccountModel(Safexchange.Models.SafexchangeDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [BindProperty]
        public UserVerification UserVerification { get; set; } = default!;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            // Check if user already has a pending or approved verification
            var existingVerification = await _context.UserVerifications
                .Where(v => v.UserId == userId && (v.Status == "Pending" || v.Status == "Approved"))
                .FirstOrDefaultAsync();

            if (existingVerification != null)
            {
                if (existingVerification.Status == "Approved")
                {
                    ErrorMessage = "Tài khoản của bạn đã được xác thực.";
                }
                else
                {
                    ErrorMessage = "Bạn đã có một yêu cầu xác thực đang chờ xử lý.";
                }
            }

            UserVerification = new UserVerification
            {
                UserId = userId,
                Status = "Pending"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            // Double check existing verification
            var existingVerification = await _context.UserVerifications
                .AnyAsync(v => v.UserId == userId && (v.Status == "Pending" || v.Status == "Approved"));

            if (existingVerification)
            {
                ErrorMessage = "Bạn không thể gửi yêu cầu mới lúc này.";
                return Page();
            }

            UserVerification.UserId = userId;
            UserVerification.Status = "Pending";
            UserVerification.CreatedAt = DateTime.Now;
            UserVerification.User = null!; // Avoid validation issues

            _context.UserVerifications.Add(UserVerification);
            await _context.SaveChangesAsync();

            SuccessMessage = "Yêu cầu xác thực của bạn đã được gửi thành công. Vui lòng chờ Admin phê duyệt.";
            return Page();
        }
    }
}
