using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Services;

namespace Safexchange.Pages.Cart;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ICartService _cart;

    public IndexModel(ICartService cart)
    {
        _cart = cart;
    }

    public IReadOnlyList<CartItem> Items => _cart.GetItems();
    public decimal Total => _cart.GetTotal();

    public IActionResult OnGetView()
    {
        return Partial("_ViewCart", this);
    }

    public IActionResult OnGetEdit()
    {
        return Partial("_EditCart", this);
    }

    public IActionResult OnGetCount()
    {
        return new JsonResult(new { count = _cart.GetItemCount() });
    }

    public IActionResult OnPostUpdate(int productId, int quantity)
    {
        _cart.UpdateQuantity(productId, quantity);
        return new JsonResult(new { success = true, count = _cart.GetItemCount(), total = _cart.GetTotal() });
    }

    public IActionResult OnPostRemove(int productId)
    {
        _cart.RemoveItem(productId);
        return new JsonResult(new { success = true, count = _cart.GetItemCount(), total = _cart.GetTotal() });
    }

    public IActionResult OnPostPrepareCheckout([FromForm] int[] productIds)
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

        _cart.SetCheckoutProductIds(productIds);

        return new JsonResult(new
        {
            success = true,
            redirectUrl = Url.Page("/Checkout/Index")
        });
    }
}
