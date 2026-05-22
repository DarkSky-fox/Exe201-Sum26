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
}
