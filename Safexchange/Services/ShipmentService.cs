using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public class ShipmentService : IShipmentService
{
    private readonly SafexchangeDbContext _db;
    private readonly IConfiguration _configuration;

    public ShipmentService(SafexchangeDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task EnsureReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        var requiredStatuses = new (string Code, string Name)[]
        {
            (ShipmentStatusCodes.PendingPickup, "Chờ lấy hàng"),
            (ShipmentStatusCodes.InTransit, "Đang giao"),
            (ShipmentStatusCodes.Delivered, "Đã giao"),
            (ShipmentStatusCodes.Cancelled, "Đã hủy")
        };

        foreach (var (code, name) in requiredStatuses)
        {
            if (!await _db.ShipStatuses.AnyAsync(s => s.StatusCode == code, cancellationToken))
            {
                _db.ShipStatuses.Add(new ShipStatus { StatusCode = code, StatusName = name });
            }
        }

        if (!await _db.ShipMethods.AnyAsync(m => m.IsActive, cancellationToken))
        {
            _db.ShipMethods.Add(new ShipMethod
            {
                MethodName = "Giao hàng nội khu",
                Description = "Giao trong khuôn viên / khu vực",
                BaseFee = 0,
                EstimatedTime = "1-2 ngày",
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryCreateShipmentForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await EnsureReferenceDataAsync(cancellationToken);

        if (await _db.Shipments.AnyAsync(s => s.OrderId == orderId, cancellationToken))
        {
            return true;
        }

        var order = await _db.Orders
            .Include(o => o.Buyer)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        if (order is null)
        {
            return false;
        }

        var deliveryAddress = await _db.UserAddresses
            .Where(a => a.UserId == order.BuyerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressId)
            .FirstOrDefaultAsync(cancellationToken);

        if (deliveryAddress is null)
        {
            return false;
        }

        var pickupAddress = await _db.UserAddresses
            .Where(a => a.UserId == order.SellerId)
            .OrderByDescending(a => a.AddressType == "pickup")
            .ThenByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressId)
            .FirstOrDefaultAsync(cancellationToken);

        pickupAddress ??= deliveryAddress;

        var pendingStatus = await GetStatusIdAsync(ShipmentStatusCodes.PendingPickup, cancellationToken);
        var shipMethod = await _db.ShipMethods.Where(m => m.IsActive).OrderBy(m => m.ShipMethodId).FirstAsync(cancellationToken);

        _db.Shipments.Add(new Shipment
        {
            OrderId = orderId,
            ShipMethodId = shipMethod.ShipMethodId,
            ShipStatusId = pendingStatus,
            PickupAddressId = pickupAddress.AddressId,
            DeliveryAddressId = deliveryAddress.AddressId,
            ShippingFee = order.ShippingFee,
            Payer = "buyer",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<IShipmentService.ShipperDeliveryItem>> GetAvailableDeliveriesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureReferenceDataAsync(cancellationToken);
        var pendingStatusId = await GetStatusIdAsync(ShipmentStatusCodes.PendingPickup, cancellationToken);

        return await _db.Shipments
            .AsNoTracking()
            .Where(s => s.ShipStatusId == pendingStatusId && s.ShipperId == null)
            .Where(s => s.Order.PaymentStatus == PaymentStatuses.Unpaid)
            .Where(s => s.Order.OrderStatus == OrderStatuses.Pending || s.Order.OrderStatus == OrderStatuses.Confirmed)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new IShipmentService.ShipperDeliveryItem
            {
                ShipmentId = s.ShipmentId,
                OrderId = s.OrderId,
                ProductTitle = s.Order.Product.Title,
                BuyerName = s.Order.Buyer.FullName,
                DeliveryAddress = s.DeliveryAddress.AddressLine,
                ReceiverPhone = s.DeliveryAddress.Phone,
                TotalAmount = s.Order.TotalAmount,
                OrderStatus = s.Order.OrderStatus,
                PaymentStatus = s.Order.PaymentStatus,
                ShipmentStatusCode = s.ShipStatus.StatusCode,
                ShipmentStatusName = s.ShipStatus.StatusName,
                CreatedAt = s.CreatedAt,
                CanAccept = true,
                CanConfirmCod = false
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IShipmentService.ShipperDeliveryItem>> GetMyActiveDeliveriesAsync(
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReferenceDataAsync(cancellationToken);
        var inTransitId = await GetStatusIdAsync(ShipmentStatusCodes.InTransit, cancellationToken);

        return await _db.Shipments
            .AsNoTracking()
            .Where(s => s.ShipperId == shipperId && s.ShipStatusId == inTransitId)
            .Where(s => s.Order.PaymentStatus == PaymentStatuses.Unpaid)
            .OrderByDescending(s => s.PickedUpAt ?? s.CreatedAt)
            .Select(s => new IShipmentService.ShipperDeliveryItem
            {
                ShipmentId = s.ShipmentId,
                OrderId = s.OrderId,
                ProductTitle = s.Order.Product.Title,
                BuyerName = s.Order.Buyer.FullName,
                DeliveryAddress = s.DeliveryAddress.AddressLine,
                ReceiverPhone = s.DeliveryAddress.Phone,
                TotalAmount = s.Order.TotalAmount,
                OrderStatus = s.Order.OrderStatus,
                PaymentStatus = s.Order.PaymentStatus,
                ShipmentStatusCode = s.ShipStatus.StatusCode,
                ShipmentStatusName = s.ShipStatus.StatusName,
                CreatedAt = s.CreatedAt,
                CanAccept = false,
                CanConfirmCod = true
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string Message)> AcceptDeliveryAsync(
        int shipmentId,
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReferenceDataAsync(cancellationToken);

        var shipment = await _db.Shipments
            .Include(s => s.Order)
            .Include(s => s.ShipStatus)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);

        if (shipment is null)
        {
            return (false, "Không tìm thấy đơn giao hàng.");
        }

        if (shipment.ShipperId.HasValue)
        {
            return (false, "Đơn này đã có shipper nhận.");
        }

        if (!string.Equals(shipment.ShipStatus.StatusCode, ShipmentStatusCodes.PendingPickup, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Đơn không ở trạng thái chờ lấy hàng.");
        }

        if (!string.Equals(shipment.Order.PaymentStatus, PaymentStatuses.Unpaid, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Đơn đã được thanh toán.");
        }

        shipment.ShipperId = shipperId;
        shipment.ShipStatusId = await GetStatusIdAsync(ShipmentStatusCodes.InTransit, cancellationToken);
        shipment.PickedUpAt = DateTime.Now;

        if (string.Equals(shipment.Order.OrderStatus, OrderStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            shipment.Order.OrderStatus = OrderStatuses.Confirmed;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (true, "Đã nhận đơn. Đơn chuyển sang trạng thái đang giao.");
    }

    public async Task<(bool Success, string Message)> ConfirmCodCollectionAsync(
        int shipmentId,
        int shipperId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReferenceDataAsync(cancellationToken);

        var shipment = await _db.Shipments
            .Include(s => s.Order)
            .Include(s => s.ShipStatus)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);

        if (shipment is null)
        {
            return (false, "Không tìm thấy đơn giao hàng.");
        }

        if (shipment.ShipperId != shipperId)
        {
            return (false, "Bạn không phải shipper của đơn này.");
        }

        if (!string.Equals(shipment.ShipStatus.StatusCode, ShipmentStatusCodes.InTransit, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Chỉ xác nhận thu tiền khi đơn đang giao.");
        }

        if (string.Equals(shipment.Order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Đơn đã được thanh toán trước đó.");
        }

        var order = shipment.Order;
        var now = DateTime.Now;

        shipment.ShipStatusId = await GetStatusIdAsync(ShipmentStatusCodes.Delivered, cancellationToken);
        shipment.DeliveredAt = now;

        order.PaymentStatus = PaymentStatuses.Paid;
        order.OrderStatus = OrderStatuses.Completed;
        order.CompletedAt = now;

        var hasPayment = await _db.Payments.AnyAsync(
            p => p.OrderId == order.OrderId && p.PaymentStatus == "success",
            cancellationToken);

        if (!hasPayment)
        {
            _db.Payments.Add(new Payment
            {
                UserId = order.BuyerId,
                OrderId = order.OrderId,
                Amount = order.TotalAmount,
                PaymentMethod = "cash",
                PaymentStatus = "success",
                TransactionCode = $"COD-{order.OrderId}-{now:yyyyMMddHHmmss}",
                PaidAt = now,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return (true, "Đã xác nhận thu tiền COD. Trạng thái đơn hàng đã cập nhật cho người mua.");
    }

    public async Task<int?> GetShipperIdForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _db.ShipperProfiles
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => (int?)s.ShipperId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> ResolveShipperIdForCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var shipperId = await GetShipperIdForUserAsync(userId, cancellationToken);
        if (shipperId.HasValue)
        {
            return shipperId.Value;
        }

        var demoUserId = _configuration.GetValue<int>("AppSettings:DemoShipperUserId", 0);
        if (demoUserId > 0)
        {
            var demoShipperId = await GetShipperIdForUserAsync(demoUserId, cancellationToken);
            if (demoShipperId.HasValue)
            {
                return demoShipperId.Value;
            }
        }

        var fallback = await _db.ShipperProfiles
            .OrderBy(s => s.ShipperId)
            .Select(s => s.ShipperId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fallback == 0)
        {
            throw new InvalidOperationException("No shipper profile in database.");
        }

        return fallback;
    }

    private async Task<int> GetStatusIdAsync(string code, CancellationToken cancellationToken)
    {
        var status = await _db.ShipStatuses.FirstAsync(s => s.StatusCode == code, cancellationToken);
        return status.ShipStatusId;
    }

}
