using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Services;

namespace Safexchange.Pages.Cart;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly ICartService _cart;
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(ICartService cart, IOrderService orderService, ICurrentUserService currentUser)
    {
        _cart = cart;
        _orderService = orderService;
        _currentUser = currentUser;
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

    public async Task<IActionResult> OnPostCheckoutAsync(CancellationToken cancellationToken)
    {
        var items = _cart.GetItems();
        if (items.Count == 0)
        {
            return new JsonResult(new { success = false, message = "Giỏ hàng trống." });
        }

        var buyerId = _currentUser.GetUserId();
        var orders = await _orderService.CreateOrdersFromCartAsync(buyerId, items, cancellationToken);

        if (orders.Count == 0)
        {
            return new JsonResult(new { success = false, message = "Không thể tạo đơn hàng. Kiểm tra lại sản phẩm." });
        }

        _cart.Clear();

        return new JsonResult(new
        {
            success = true,
            message = $"Đã tạo {orders.Count} đơn hàng thành công.",
            orderIds = orders.Select(o => o.OrderId).ToList(),
            redirectUrl = Url.Page("/Orders/Index")
        });
    }
}
