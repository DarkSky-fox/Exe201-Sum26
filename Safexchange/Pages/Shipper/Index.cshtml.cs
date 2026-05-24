using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Services;

namespace Safexchange.Pages.Shipper;

[Authorize(Roles = "shipper")]
public class IndexModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(IShipmentService shipmentService, ICurrentUserService currentUser)
    {
        _shipmentService = shipmentService;
        _currentUser = currentUser;
    }

    public IReadOnlyList<ShipperOrderItem> AvailableOrders { get; private set; } = Array.Empty<ShipperOrderItem>();

    public IReadOnlyList<ShipperOrderItem> MyOrders { get; private set; } = Array.Empty<ShipperOrderItem>();

    public string? Message { get; private set; }

    public bool IsError { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadPageAsync(cancellationToken);
    }

    public Task<IActionResult> OnPostTakeAsync(int shipmentId, CancellationToken cancellationToken)
        => RunActionAsync(shipmentId, _shipmentService.AssignShipmentAsync, cancellationToken);

    public Task<IActionResult> OnPostAdvanceAsync(int shipmentId, CancellationToken cancellationToken)
        => RunActionAsync(shipmentId, _shipmentService.AdvanceShipmentStatusAsync, cancellationToken);

    public Task<IActionResult> OnPostConfirmCodAsync(int shipmentId, CancellationToken cancellationToken)
        => RunActionAsync(shipmentId, _shipmentService.ConfirmCodPaymentAsync, cancellationToken);

    private async Task<IActionResult> RunActionAsync(
        int shipmentId,
        Func<int, int, CancellationToken, Task<(bool Success, string Message)>> action,
        CancellationToken cancellationToken)
    {
        var shipperId = await _shipmentService.GetShipperIdForUserAsync(_currentUser.GetUserId(), cancellationToken);
        if (!shipperId.HasValue)
        {
            Message = "Tài khoản chưa có hồ sơ shipper. Liên hệ quản trị viên.";
            IsError = true;
            return await LoadPageAsync(cancellationToken);
        }

        var result = await action(shipmentId, shipperId.Value, cancellationToken);
        Message = result.Message;
        IsError = !result.Success;
        return await LoadPageAsync(cancellationToken);
    }

    private async Task<IActionResult> LoadPageAsync(CancellationToken cancellationToken)
    {
        var shipperId = await _shipmentService.GetShipperIdForUserAsync(_currentUser.GetUserId(), cancellationToken);
        if (!shipperId.HasValue)
        {
            AvailableOrders = Array.Empty<ShipperOrderItem>();
            MyOrders = Array.Empty<ShipperOrderItem>();
            Message ??= "Tài khoản chưa được liên kết hồ sơ shipper.";
            IsError = true;
            return Page();
        }

        var orders = await _shipmentService.GetShipperOrdersAsync(shipperId.Value, cancellationToken);
        AvailableOrders = orders.AvailableOrders;
        MyOrders = orders.MyOrders;
        return Page();
    }
}
