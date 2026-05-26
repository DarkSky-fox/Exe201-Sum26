using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Safexchange.Pages;

public class GoogleLoginModel : PageModel
{
    public IActionResult OnGet()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Page("/GoogleCallback")
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
}
