using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Pages.Admin.UserManagement
{
    public class PendingVerificationsModel : PageModel
    {
        private readonly Safexchange.Models.SafexchangeDbContext _context;

        public PendingVerificationsModel(Safexchange.Models.SafexchangeDbContext context)
        {
            _context = context;
        }

        public IList<UserVerification> UserVerifications { get; set; } = default!;

        public async Task OnGetAsync()
        {
            UserVerifications = await _context.UserVerifications
                .Include(u => u.User)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var verification = await _context.UserVerifications.FindAsync(id);
            if (verification == null)
            {
                return NotFound();
            }

            verification.Status = "Approved";
            verification.VerifiedAt = DateTime.Now;
            // Assuming current user is Admin, but for now we just mark it
            // verification.ReviewedBy = ...; 

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string reason)
        {
            var verification = await _context.UserVerifications.FindAsync(id);
            if (verification == null)
            {
                return NotFound();
            }

            verification.Status = "Rejected";
            verification.RejectionReason = reason;
            verification.VerifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
