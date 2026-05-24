using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public class ShipmentService : IShipmentService
{
    private readonly SafexchangeDbContext _db;
    private readonly ICheckoutService _checkoutService;

    public ShipmentService(SafexchangeDbContext db, ICheckoutService checkoutService)
    {
        _db = db;
        _checkoutService = checkoutService;
    }

    public async Task<int?> GetShipperIdForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var profile = await _db.ShipperProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (profile is not null)
        {
            return profile.ShipperId;
        }

        var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user is null || !string.Equals(user.Role, "shipper", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        profile = new ShipperProfile
        {
            UserId = userId,
            VehicleType = "Xe máy",
            ShipperStatus = "available",
            RatingAvg = 0
        };

        _db.ShipperProfiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);
        return profile.ShipperId;
    }

    public async Task<IReadOnlyList<ShipperOrderItem>> GetShipperOrdersAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        await _checkoutService.EnsureReferenceDataAsync(cancellationToken);

        var shipments = await _db.Shipments
            .AsNoTracking()
            .Include(s => s.Order)
                .ThenInclude(o => o.Product)
            .Include(s => s.Order)
                .ThenInclude(o => o.Buyer)
            .Include(s => s.DeliveryAddress)
            .Include(s => s.ShipStatus)
            .Where(s => s.ShipperId == null || s.ShipperId == shipperId)
            .Where(s => s.ShipStatus!.StatusCode != "done")
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return shipments.Select(s =>
        {
            var code = s.ShipStatus?.StatusCode ?? "";
            var assigned = s.ShipperId == shipperId;

            return new ShipperOrderItem
            {
                ShipmentId = s.ShipmentId,
                OrderId = s.OrderId,
                ProductTitle = s.Order.Product.Title,
                BuyerName = s.Order.Buyer.FullName,
                DeliveryAddress = s.DeliveryAddress.AddressLine,
                DeliveryPhone = s.DeliveryAddress.Phone,
                TotalAmount = s.Order.TotalAmount,
                OrderStatus = s.Order.OrderStatus,
                PaymentStatus = s.Order.PaymentStatus,
                ShipStatusCode = code,
                ShipStatusName = s.ShipStatus?.StatusName ?? code,
                IsAssignedToMe = assigned,
                CanTakeOrder = s.ShipperId is null && code == "waiting",
                CanStartDelivery = assigned && code == "waiting",
                CanMarkDelivered = assigned && code == "delivering",
                CanConfirmCod = assigned && code == "delivered" && s.Order.PaymentStatus == "unpaid"
            };
        }).ToList();
    }

    public async Task<(bool Success, string Message)> AssignShipmentAsync(
        int shipmentId,
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        var shipment = await GetEditableShipmentAsync(shipmentId, shipperId, allowUnassigned: true, cancellationToken);
        if (shipment is null)
        {
            return (false, "Không tìm thấy đơn hoặc đơn đã được shipper khác nhận.");
        }

        if (shipment.ShipperId.HasValue && shipment.ShipperId != shipperId)
        {
            return (false, "Đơn đã được shipper khác nhận.");
        }

        shipment.ShipperId = shipperId;
        await _db.SaveChangesAsync(cancellationToken);
        return (true, "Đã nhận đơn giao hàng.");
    }

    public async Task<(bool Success, string Message)> AdvanceShipmentStatusAsync(
        int shipmentId,
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        var shipment = await GetEditableShipmentAsync(shipmentId, shipperId, allowUnassigned: false, cancellationToken);
        if (shipment is null)
        {
            return (false, "Không tìm thấy đơn giao hàng.");
        }

        var currentCode = shipment.ShipStatus?.StatusCode ?? "";
        string? nextCode = currentCode switch
        {
            "waiting" => "delivering",
            "delivering" => "delivered",
            _ => null
        };

        if (nextCode is null)
        {
            return (false, "Không thể cập nhật trạng thái từ bước hiện tại.");
        }

        var nextStatus = await _db.ShipStatuses
            .FirstAsync(s => s.StatusCode == nextCode, cancellationToken);

        shipment.ShipStatusId = nextStatus.ShipStatusId;
        var now = DateTime.Now;

        if (nextCode == "delivering")
        {
            shipment.PickedUpAt ??= now;
            // DB chỉ cho phép: pending, confirmed, completed, cancelled, returned
            shipment.Order.OrderStatus = "confirmed";
        }
        else if (nextCode == "delivered")
        {
            shipment.DeliveredAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return (true, nextCode switch
        {
            "delivering" => "Đã bắt đầu giao hàng.",
            "delivered" => "Đã giao hàng cho khách. Chờ thu tiền COD.",
            _ => "Cập nhật trạng thái thành công."
        });
    }

    public async Task<(bool Success, string Message)> ConfirmCodPaymentAsync(
        int shipmentId,
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        var shipment = await GetEditableShipmentAsync(shipmentId, shipperId, allowUnassigned: false, cancellationToken);
        if (shipment is null)
        {
            return (false, "Không tìm thấy đơn giao hàng.");
        }

        if (shipment.ShipStatus?.StatusCode != "delivered")
        {
            return (false, "Chỉ xác nhận thu tiền sau khi đã giao hàng cho khách.");
        }

        var doneStatus = await _db.ShipStatuses
            .FirstAsync(s => s.StatusCode == "done", cancellationToken);

        var now = DateTime.Now;
        shipment.ShipStatusId = doneStatus.ShipStatusId;
        shipment.Order.OrderStatus = "completed";
        shipment.Order.PaymentStatus = "paid";
        shipment.Order.CompletedAt = now;

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == shipment.OrderId, cancellationToken);

        if (payment is not null)
        {
            payment.PaymentStatus = "success";
            payment.PaidAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (true, "Đã xác nhận thu tiền COD. Đơn hàng hoàn tất.");
    }

    private async Task<Shipment?> GetEditableShipmentAsync(
        int shipmentId,
        int shipperId,
        bool allowUnassigned,
        CancellationToken cancellationToken)
    {
        var shipment = await _db.Shipments
            .Include(s => s.ShipStatus)
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);

        if (shipment is null)
        {
            return null;
        }

        if (allowUnassigned && shipment.ShipperId is null)
        {
            return shipment;
        }

        return shipment.ShipperId == shipperId ? shipment : null;
    }
}
