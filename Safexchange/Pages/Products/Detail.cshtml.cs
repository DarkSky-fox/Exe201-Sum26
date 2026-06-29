using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Products;

[IgnoreAntiforgeryToken]
public class DetailModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICartService _cart;
    private readonly ICartAddService _cartAdd;
    private readonly ICurrentUserService _currentUser;

    public DetailModel(
        SafexchangeDbContext db,
        ICartService cart,
        ICartAddService cartAdd,
        ICurrentUserService currentUser)
    {
        _db = db;
        _cart = cart;
        _cartAdd = cartAdd;
        _currentUser = currentUser;
    }

    public Product? Product { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public bool IsAvailableForPurchase { get; private set; } = true;
    public bool IsFavourited { get; private set; } = false;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        Product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.Seller)
            .Include(p => p.ProductImages)
            .Include(p => p.Combos)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (Product is null)
        {
            return NotFound();
        }

        CoverImageUrl = Product.ProductImages
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();

        IsAvailableForPurchase = ProductStatusHelper.IsDisplayStatus(Product.Status);

        // Check if user has favourited this product
        var userId = _currentUser.GetUserId();
        if (userId != 0)
        {
            IsFavourited = await _db.Favourites
                .AnyAsync(f => f.UserId == userId && f.ProductId == id, cancellationToken);
        }

        Product.ViewCount++;
        await _db.SaveChangesAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity = 1)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Vui lòng đăng nhập để thêm vào giỏ.",
                redirectUrl = "/Login"
            });
        }

        var result = await _cartAdd.AddProductAsync(_currentUser.GetUserId(), id, quantity);
        return new JsonResult(new
        {
            success = result.Success,
            message = result.Message,
            cartCount = result.CartCount,
            redirectUrl = result.RedirectUrl
        });
    }

    public async Task<IActionResult> OnPostBuyNowAsync(int id, int quantity = 1)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để mua hàng.", redirectUrl = "/Login" });
        }

        var result = await _cartAdd.AddProductAsync(_currentUser.GetUserId(), id, quantity);
        if (!result.Success)
        {
            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message,
                cartCount = result.CartCount
            });
        }

        _cart.SetCheckoutProductIds(new[] { id });

        return new JsonResult(new
        {
            success = true,
            message = "Chuyển đến trang thanh toán...",
            cartCount = result.CartCount,
            redirectUrl = Url.Page("/Checkout/Index")
        });
    }

    public async Task<IActionResult> OnGetToggleFavouriteAsync(int id)
    {
        var userId = _currentUser.GetUserId();
        if (userId == 0)
        {
            return new JsonResult(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var existing = await _db.Favourites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == id);

        if (existing != null)
        {
            _db.Favourites.Remove(existing);
            await _db.SaveChangesAsync();
            return new JsonResult(new { success = true, isFavourited = false });
        }
        else
        {
            var favourite = new Favourite
            {
                UserId = userId,
                ProductId = id,
                CreatedAt = DateTime.Now
            };
            _db.Favourites.Add(favourite);
            await _db.SaveChangesAsync();
            return new JsonResult(new { success = true, isFavourited = true });
        }
    }
}
