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
    public class VerifyModel : PageModel
    {
        private readonly Safexchange.Models.SafexchangeDbContext _context;

        public VerifyModel(Safexchange.Models.SafexchangeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserVerification UserVerification { get; set; } = default!;

        public User TargetUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TargetUser = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);

            if (TargetUser == null)
            {
                return NotFound();
            }

            UserVerification = new UserVerification
            {
                UserId = TargetUser.UserId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Set the navigation properties to avoid validation errors if necessary, 
            // but we really only need the UserId.
            
            // To be safe, we fetch the user again or ensure UserId is valid
            var userExists = await _context.Users.AnyAsync(u => u.UserId == UserVerification.UserId);
            if (!userExists)
            {
                return NotFound();
            }

            UserVerification.CreatedAt = DateTime.Now;
            
            // Explicitly set null for navigation property to avoid EF issues if it's bound but empty
            UserVerification.User = null!; 

            _context.UserVerifications.Add(UserVerification);
            await _context.SaveChangesAsync();

            return RedirectToPage("./PendingVerifications");
        }
    }
}
