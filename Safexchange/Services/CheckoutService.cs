using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public class CheckoutService : ICheckoutService
{
    private static readonly (string Code, string Name)[] DefaultShipStatuses =
    {
        ("waiting", "Chờ giao hàng"),
        ("delivering", "Đang giao hàng"),
        ("delivered", "Đã giao — chờ thu tiền"),
        ("done", "Hoàn tất giao hàng")
    };

    private readonly SafexchangeDbContext _db;
    private readonly IOrderService _orderService;

    public CheckoutService(SafexchangeDbContext db, IOrderService orderService)
    {
        _db = db;
        _orderService = orderService;
    }

    public async Task EnsureReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (code, name) in DefaultShipStatuses)
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
                MethodName = "Giao hàng tiêu chuẩn",
                Description = "Giao trong 2-3 ngày",
                BaseFee = 20000,
                EstimatedTime = "2-3 ngày",
                IsActive = true
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CheckoutResult> PlaceOrderAsync(CheckoutInput input, CancellationToken cancellationToken = default)
    {
        if (input.Items.Count == 0)
        {
            return new CheckoutResult { Success = false, Message = "Không có sản phẩm để thanh toán." };
        }

        if (string.IsNullOrWhiteSpace(input.ReceiverName)
            || string.IsNullOrWhiteSpace(input.Phone)
            || string.IsNullOrWhiteSpace(input.AddressLine))
        {
            return new CheckoutResult { Success = false, Message = "Vui lòng nhập đầy đủ thông tin người nhận và địa chỉ giao hàng." };
        }

        await EnsureReferenceDataAsync(cancellationToken);

        var shipMethod = await _db.ShipMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.ShipMethodId)
            .FirstAsync(cancellationToken);

        var waitingStatus = await _db.ShipStatuses
            .FirstAsync(s => s.StatusCode == "waiting", cancellationToken);

        var buyer = await _db.Users.FindAsync(new object[] { input.BuyerId }, cancellationToken);
        if (buyer is null)
        {
            return new CheckoutResult { Success = false, Message = "Không tìm thấy tài khoản người mua." };
        }

        var deliveryAddress = await SaveDeliveryAddressAsync(
            input.BuyerId,
            input.ReceiverName.Trim(),
            input.Phone.Trim(),
            input.AddressLine.Trim(),
            cancellationToken);

        var orderIds = new List<int>();
        var now = DateTime.Now;

        foreach (var item in input.Items)
        {
            var product = await _db.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId, cancellationToken);

            if (product is null || product.SellerId == input.BuyerId)
            {
                continue;
            }

            var itemPrice = product.Price * item.Quantity;
            var platformFee = await _orderService.CalculatePlatformFeeAsync(itemPrice, cancellationToken);
            var shippingFee = shipMethod.BaseFee;
            var totalAmount = itemPrice + platformFee + shippingFee;

            var pickupAddress = await GetOrCreatePickupAddressAsync(product.Seller, cancellationToken);

            var order = new Order
            {
                BuyerId = input.BuyerId,
                SellerId = product.SellerId,
                ProductId = product.ProductId,
                ItemPrice = itemPrice,
                PlatformFee = platformFee,
                DiscountAmount = 0,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                OrderStatus = "confirmed",
                PaymentStatus = "unpaid",
                CreatedAt = now
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            _db.Payments.Add(new Payment
            {
                UserId = input.BuyerId,
                OrderId = order.OrderId,
                Amount = totalAmount,
                PaymentMethod = "cash",
                PaymentStatus = "pending",
                CreatedAt = now
            });

            _db.Shipments.Add(new Shipment
            {
                OrderId = order.OrderId,
                ShipMethodId = shipMethod.ShipMethodId,
                ShipStatusId = waitingStatus.ShipStatusId,
                PickupAddressId = pickupAddress.AddressId,
                DeliveryAddressId = deliveryAddress.AddressId,
                ShippingFee = shippingFee,
                Payer = "buyer",
                CreatedAt = now
            });

            orderIds.Add(order.OrderId);
        }

        if (orderIds.Count == 0)
        {
            return new CheckoutResult
            {
                Success = false,
                Message = "Không thể tạo đơn hàng. Kiểm tra lại sản phẩm trong giỏ."
            };
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new CheckoutResult
        {
            Success = true,
            Message = $"Đặt hàng thành công {orderIds.Count} đơn.",
            OrderIds = orderIds
        };
    }

    private async Task<UserAddress> SaveDeliveryAddressAsync(
        int userId,
        string receiverName,
        string phone,
        string addressLine,
        CancellationToken cancellationToken)
    {
        var existing = await _db.UserAddresses
            .Where(a => a.UserId == userId
                && a.AddressType == "delivery"
                && a.ReceiverName == receiverName
                && a.Phone == phone
                && a.AddressLine == addressLine)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var hasDefault = await _db.UserAddresses
            .AnyAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);

        var address = new UserAddress
        {
            UserId = userId,
            ReceiverName = receiverName,
            Phone = phone,
            AddressLine = addressLine,
            AddressType = "delivery",
            IsDefault = !hasDefault
        };

        _db.UserAddresses.Add(address);
        await _db.SaveChangesAsync(cancellationToken);
        return address;
    }

    private async Task<UserAddress> GetOrCreatePickupAddressAsync(User seller, CancellationToken cancellationToken)
    {
        var pickup = await _db.UserAddresses
            .Where(a => a.UserId == seller.UserId && a.AddressType == "pickup")
            .OrderByDescending(a => a.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (pickup is not null)
        {
            return pickup;
        }

        pickup = new UserAddress
        {
            UserId = seller.UserId,
            ReceiverName = seller.FullName,
            Phone = seller.Phone ?? "0000000000",
            AddressLine = "Địa chỉ lấy hàng — cập nhật trong hồ sơ người bán",
            AddressType = "pickup",
            IsDefault = true
        };

        _db.UserAddresses.Add(pickup);
        await _db.SaveChangesAsync(cancellationToken);
        return pickup;
    }
}
