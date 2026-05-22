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
    private readonly ICurrentUserService _currentUser;

    public DetailModel(SafexchangeDbContext db, ICartService cart, ICurrentUserService currentUser)
    {
        _db = db;
        _cart = cart;
        _currentUser = currentUser;
    }

    public Product? Product { get; private set; }
    public string? CoverImageUrl { get; private set; }

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

        Product.ViewCount++;
        await _db.SaveChangesAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity = 1)
    {
        var (success, message) = await TryAddToCartAsync(id, quantity);
        return new JsonResult(new
        {
            success,
            message,
            cartCount = _cart.GetItemCount()
        });
    }

    public async Task<IActionResult> OnPostBuyNowAsync(int id, int quantity = 1)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return new JsonResult(new { success = false, message = "Vui lòng đăng nhập để mua hàng.", redirectUrl = "/Login" });
        }

        var (success, message) = await TryAddToCartAsync(id, quantity);
        if (!success)
        {
            return new JsonResult(new { success, message, cartCount = _cart.GetItemCount() });
        }

        _cart.SetCheckoutProductIds(new[] { id });

        return new JsonResult(new
        {
            success = true,
            message = "Chuyển đến trang thanh toán...",
            cartCount = _cart.GetItemCount(),
            redirectUrl = Url.Page("/Checkout/Index")
        });
    }

    private async Task<(bool Success, string Message)> TryAddToCartAsync(int id, int quantity)
    {
        var product = await _db.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null)
        {
            return (false, "Sản phẩm không tồn tại.");
        }

        if (product.SellerId == _currentUser.GetUserId())
        {
            return (false, "Bạn không thể mua sản phẩm của chính mình.");
        }

        var imageUrl = product.ProductImages
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();

        _cart.AddItem(new CartItem
        {
            ProductId = product.ProductId,
            SellerId = product.SellerId,
            Title = product.Title,
            ImageUrl = imageUrl,
            UnitPrice = product.Price,
            Quantity = Math.Max(1, quantity)
        });

        return (true, "Đã thêm vào giỏ hàng.");
    }
}
