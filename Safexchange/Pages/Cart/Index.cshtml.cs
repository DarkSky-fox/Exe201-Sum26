using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Cart;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ICartService _cart;
    private readonly ICartAddService _cartAdd;
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(
        ICartService cart,
        ICartAddService cartAdd,
        SafexchangeDbContext db,
        ICurrentUserService currentUser)
    {
        _cart = cart;
        _cartAdd = cartAdd;
        _db = db;
        _currentUser = currentUser;
    }

    public IReadOnlyList<CartItem> Items => _cart.GetItems();
    public decimal Total => _cart.GetTotal();

    public IActionResult OnGetView()
    {
        return Partial("_ViewCart", this);
    }

    public IActionResult OnGetCount()
    {
        return new JsonResult(new { count = _cart.GetItemCount() });
    }

    public async Task<IActionResult> OnPostAdd(int productId)
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

        var result = await _cartAdd.AddProductAsync(_currentUser.GetUserId(), productId, 1);
        return new JsonResult(new
        {
            success = result.Success,
            message = result.Message,
            cartCount = result.CartCount,
            redirectUrl = result.RedirectUrl
        });
    }

    public IActionResult OnPostRemove(int productId)
    {
        _cart.RemoveItem(productId);
        return new JsonResult(new { success = true, count = _cart.GetItemCount(), total = _cart.GetTotal() });
    }

    public async Task<IActionResult> OnPostPrepareCheckout([FromForm] int[] productIds, CancellationToken cancellationToken)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return new JsonResult(new { success = false, message = "Vui lòng đăng nhập.", redirectUrl = "/Login" });
        }

        if (productIds is null || productIds.Length == 0)
        {
            return new JsonResult(new { success = false, message = "Chọn ít nhất một sản phẩm để thanh toán." });
        }

        var items = _cart.GetItems(productIds);
        if (items.Count == 0)
        {
            return new JsonResult(new { success = false, message = "Sản phẩm đã chọn không còn trong giỏ hàng." });
        }

        var displayStatusIds = await _db.ProductStatuses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var allowedStatusIds = displayStatusIds
            .Where(ProductStatusHelper.IsDisplayStatus)
            .Select(s => s.StatusId)
            .ToHashSet();

        var validProductIds = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductId))
            .Where(p => allowedStatusIds.Contains(p.StatusId))
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken);

        var unavailableProductIds = productIds.Except(validProductIds).ToList();
        if (unavailableProductIds.Count > 0)
        {
            _cart.RemoveItems(unavailableProductIds);
        }

        if (validProductIds.Count == 0)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Sản phẩm bạn chọn đã hết hàng hoặc đã bị ẩn. Vui lòng chọn sản phẩm khác."
            });
        }

        _cart.SetCheckoutProductIds(validProductIds);

        return new JsonResult(new
        {
            success = true,
            message = unavailableProductIds.Count > 0 ? "Một số sản phẩm đã hết hàng và được gỡ khỏi giỏ." : null,
            redirectUrl = Url.Page("/Checkout/Index")
        });
    }
}
