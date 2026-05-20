using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Orders;

[IgnoreAntiforgeryToken]
public class EditModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUser;

    public EditModel(IOrderService orderService, ICurrentUserService currentUser)
    {
        _orderService = orderService;
        _currentUser = currentUser;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Order? Order { get; private set; }
    public string? VoucherCode { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Order = await _orderService.GetOrderForBuyerAsync(Id, _currentUser.GetUserId(), cancellationToken);
        if (Order is null)
        {
            return NotFound();
        }

        VoucherCode = Order.Voucher?.VoucherCode;
        return Partial("_EditOrder", this);
    }

    public async Task<IActionResult> OnPostAsync(decimal shippingFee, string? voucherCode, CancellationToken cancellationToken)
    {
        var ok = await _orderService.UpdateOrderAsync(Id, _currentUser.GetUserId(), shippingFee, voucherCode, cancellationToken);
        if (!ok)
        {
            return new JsonResult(new { success = false, message = "Không thể cập nhật đơn hàng (chỉ sửa được đơn đang chờ xử lý)." });
        }

        return new JsonResult(new { success = true, message = "Đã cập nhật đơn hàng." });
    }
}
