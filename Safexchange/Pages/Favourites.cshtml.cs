using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages;

[IgnoreAntiforgeryToken]
public class FavouritesModel : PageModel
{
    private readonly SafexchangeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public FavouritesModel(SafexchangeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public List<Product> FavouriteProducts { get; set; } = new();
    public int FavouriteCount { get; set; }

    public void OnGet()
    {
        var userId = _currentUser.GetUserId();
        // If userId is 0 or demo default (1), redirect to login
        if (userId == 0)
        {
            Response.Redirect("/Login");
            return;
        }

        FavouriteProducts = _context.Favourites
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
            .ThenInclude(p => p.Category)
            .Include(f => f.Product)
            .ThenInclude(p => p.ProductImages)
            .Include(f => f.Product)
            .ThenInclude(p => p.Status)
            .Select(f => f.Product!)
            .ToList();

        FavouriteCount = FavouriteProducts.Count;
    }

    public IActionResult OnPostRemove(int productId)
    {
        var userId = _currentUser.GetUserId();
        if (userId == 0)
        {
            return RedirectToPage("/Login");
        }

        var favourite = _context.Favourites
            .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

        if (favourite != null)
        {
            _context.Favourites.Remove(favourite);
            _context.SaveChanges();
        }

        return RedirectToPage();
    }

    public IActionResult OnGetToggle(int productId)
    {
        var userId = _currentUser.GetUserId();
        if (userId == 0)
        {
            return new JsonResult(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var existing = _context.Favourites
            .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

        if (existing != null)
        {
            _context.Favourites.Remove(existing);
            _context.SaveChanges();
            return new JsonResult(new { success = true, isFavourited = false });
        }
        else
        {
            var favourite = new Favourite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            };
            _context.Favourites.Add(favourite);
            _context.SaveChanges();
            return new JsonResult(new { success = true, isFavourited = true });
        }
    }

    public IActionResult OnGetCount()
    {
        var userId = _currentUser.GetUserId();
        if (userId == 0)
        {
            return new JsonResult(new { count = 0 });
        }

        var count = _context.Favourites.Count(f => f.UserId == userId);
        return new JsonResult(new { count });
    }

    public IActionResult OnGetCheck(int productId)
    {
        var userId = _currentUser.GetUserId();
        if (userId == 0)
        {
            return new JsonResult(new { isFavourited = false });
        }

        var isFavourited = _context.Favourites.Any(f => f.UserId == userId && f.ProductId == productId);
        return new JsonResult(new { isFavourited });
    }
}
