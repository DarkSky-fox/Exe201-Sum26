using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(SafexchangeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public User? User { get; set; }
    public UserVerification? Verification { get; set; }
    public int TotalProducts { get; set; }
    public int TotalSold { get; set; }
    public int TotalOrders { get; set; }
    public double AverageRating { get; set; }

    public void OnGet()
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
        {
            Response.Redirect("/Login");
            return;
        }

        User = _context.Users.FirstOrDefault(u => u.UserId == userId);
        Verification = _context.UserVerifications
            .FirstOrDefault(v => v.UserId == userId);

        TotalProducts = _context.Products.Count(p => p.SellerId == userId);
        TotalSold = _context.Products.Count(p => p.SellerId == userId && p.Status != null && p.Status.StatusName == "Đã bán");
        
        var buyerOrders = _context.Orders.Count(o => o.BuyerId == userId);
        var sellerOrders = _context.Orders.Count(o => o.SellerId == userId);
        TotalOrders = buyerOrders + sellerOrders;

        var ratings = _context.Ratings.Where(r => r.RevieweeId == userId).ToList();
        AverageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.RatingScore), 1) : 0;
    }

    public async Task<IActionResult> OnPostAsync(string fullName, string phone)
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
        {
            return RedirectToPage("/Login");
        }

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            return Page();
        }

        user.FullName = fullName;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            user.Phone = phone;
        }

        _context.SaveChanges();
        TempData["Success"] = "Cập nhật hồ sơ thành công!";
        return RedirectToPage();
    }
}
