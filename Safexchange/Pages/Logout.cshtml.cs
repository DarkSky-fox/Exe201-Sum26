using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Services;

namespace Safexchange.Pages;

public class LogoutModel : PageModel
{
    private readonly ICurrentUserService _currentUser;

    public LogoutModel(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public IActionResult OnGet()
    {
        _currentUser.SignOut();
        return RedirectToPage("/Login");
    }
}
