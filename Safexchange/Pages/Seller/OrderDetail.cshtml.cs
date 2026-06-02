using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Seller;

[Authorize]
public class OrderDetailModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUserService;

    public OrderDetailModel(IOrderService orderService, ICurrentUserService currentUserService)
    {
        _orderService = orderService;
        _currentUserService = currentUserService;
    }

    public Order? Order { get; private set; }

    [BindProperty]
    public string NewStatus { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int productId, CancellationToken cancellationToken)
    {
        var sellerId = _currentUserService.GetUserId();
        Order = await _orderService.GetOrderForSellerByProductIdAsync(productId, sellerId, cancellationToken);

        if (Order is null)
        {
            return NotFound();
        }

        NewStatus = Order.OrderStatus;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int orderId, CancellationToken cancellationToken)
    {
        var sellerId = _currentUserService.GetUserId();
        var success = await _orderService.UpdateOrderStatusAsync(orderId, sellerId, NewStatus, cancellationToken);

        if (!success)
        {
            TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng.";
            return RedirectToPage(new { productId = Request.Query["productId"] });
        }

        TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
        return RedirectToPage(new { productId = Request.Query["productId"] });
    }
}
