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
    public class IndexModel : PageModel
    {
        private readonly Safexchange.Models.SafexchangeDbContext _context;

        public IndexModel(Safexchange.Models.SafexchangeDbContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;

        public async Task OnGetAsync()
        {
            User = await _context.Users.ToListAsync();
        }
    }
}
