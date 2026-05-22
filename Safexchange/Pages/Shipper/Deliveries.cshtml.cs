using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Services;

namespace Safexchange.Pages.Shipper;

[IgnoreAntiforgeryToken]
public class DeliveriesModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ICurrentUserService _currentUser;

    public DeliveriesModel(IShipmentService shipmentService, ICurrentUserService currentUser)
    {
        _shipmentService = shipmentService;
        _currentUser = currentUser;
    }

    public IList<IShipmentService.ShipperDeliveryItem> Available { get; private set; } = new List<IShipmentService.ShipperDeliveryItem>();
    public IList<IShipmentService.ShipperDeliveryItem> Active { get; private set; } = new List<IShipmentService.ShipperDeliveryItem>();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadListsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAcceptAsync(int shipmentId, CancellationToken cancellationToken)
    {
        var shipperId = await ResolveShipperIdAsync(cancellationToken);
        if (shipperId is null)
        {
            return JsonResult(false, "Chưa cấu hình tài khoản shipper.");
        }

        var (success, message) = await _shipmentService.AcceptDeliveryAsync(shipmentId, shipperId.Value, cancellationToken);
        return JsonResult(success, message);
    }

    public async Task<IActionResult> OnPostConfirmCodAsync(int shipmentId, CancellationToken cancellationToken)
    {
        var shipperId = await ResolveShipperIdAsync(cancellationToken);
        if (shipperId is null)
        {
            return JsonResult(false, "Chưa cấu hình tài khoản shipper.");
        }

        var (success, message) = await _shipmentService.ConfirmCodCollectionAsync(shipmentId, shipperId.Value, cancellationToken);
        return JsonResult(success, message);
    }

    private async Task LoadListsAsync(CancellationToken cancellationToken)
    {
        var shipperId = await ResolveShipperIdAsync(cancellationToken);
        if (shipperId is null)
        {
            ErrorMessage = "Không tìm thấy hồ sơ shipper. Thêm Shipper_Profile trong database hoặc cấu hình AppSettings:DemoShipperUserId.";
            return;
        }

        Available = (await _shipmentService.GetAvailableDeliveriesAsync(cancellationToken)).ToList();
        Active = (await _shipmentService.GetMyActiveDeliveriesAsync(shipperId.Value, cancellationToken)).ToList();
    }

    private async Task<int?> ResolveShipperIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var shipperId = await _shipmentService.GetShipperIdForUserAsync(userId, cancellationToken);
            if (shipperId.HasValue)
            {
                return shipperId;
            }

            return await _shipmentService.ResolveShipperIdForCurrentUserAsync(userId, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private JsonResult JsonResult(bool success, string message) =>
        new(new { success, message });
}
