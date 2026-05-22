using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public class OrderService : IOrderService
{
    private readonly SafexchangeDbContext _db;

    public OrderService(SafexchangeDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> CalculatePlatformFeeAsync(decimal itemPrice, CancellationToken cancellationToken = default)
    {
        var rule = await _db.FeeRules
            .Where(r => r.IsActive
                && r.MinOrderValue <= itemPrice
                && (r.MaxOrderValue == null || r.MaxOrderValue >= itemPrice))
            .OrderByDescending(r => r.MinOrderValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (rule is null)
        {
            return Math.Round(itemPrice * 0.05m, 0);
        }

        return rule.FeeType.ToLowerInvariant() switch
        {
            "percent" or "percentage" => Math.Round(itemPrice * rule.FeeValue / 100m, 0),
            _ => rule.FeeValue
        };
    }

    public Task<decimal> CalculateDiscountAsync(Voucher? voucher, decimal itemPrice, CancellationToken cancellationToken = default)
    {
        if (voucher is null || !voucher.IsActive)
        {
            return Task.FromResult(0m);
        }

        var now = DateTime.Now;
        if (now < voucher.StartAt || now > voucher.EndAt || itemPrice < voucher.MinOrderValue)
        {
            return Task.FromResult(0m);
        }

        decimal discount = voucher.DiscountType.ToLowerInvariant() switch
        {
            "percent" or "percentage" => itemPrice * voucher.DiscountValue / 100m,
            _ => voucher.DiscountValue
        };

        if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
        {
            discount = voucher.MaxDiscountAmount.Value;
        }

        return Task.FromResult(Math.Round(discount, 0));
    }

    public async Task<List<Order>> CreateOrdersFromCartAsync(int buyerId, IReadOnlyList<CartItem> items, CancellationToken cancellationToken = default)
    {
        var created = new List<Order>();

        foreach (var item in items)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId, cancellationToken);

            if (product is null || product.SellerId == buyerId)
            {
                continue;
            }

            var itemPrice = product.Price * item.Quantity;
            var platformFee = await CalculatePlatformFeeAsync(itemPrice, cancellationToken);

            var order = new Order
            {
                BuyerId = buyerId,
                SellerId = product.SellerId,
                ProductId = product.ProductId,
                ItemPrice = itemPrice,
                PlatformFee = platformFee,
                DiscountAmount = 0,
                ShippingFee = 0,
                TotalAmount = itemPrice + platformFee,
                OrderStatus = "pending",
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            created.Add(order);
        }

        if (created.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    public async Task<Order?> GetOrderForBuyerAsync(int orderId, int buyerId, CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .Include(o => o.Product)
                .ThenInclude(p => p.ProductImages)
            .Include(o => o.Voucher)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BuyerId == buyerId, cancellationToken);
    }

    public async Task<bool> UpdateOrderAsync(int orderId, int buyerId, decimal shippingFee, string? voucherCode, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BuyerId == buyerId, cancellationToken);

        if (order is null || !string.Equals(order.OrderStatus, "pending", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Voucher? voucher = null;
        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            voucher = await _db.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherCode == voucherCode.Trim(), cancellationToken);
        }

        var discount = await CalculateDiscountAsync(voucher, order.ItemPrice, cancellationToken);

        order.VoucherId = voucher?.VoucherId;
        order.DiscountAmount = discount;
        order.ShippingFee = Math.Max(0, shippingFee);
        order.TotalAmount = order.ItemPrice + order.PlatformFee + order.ShippingFee - order.DiscountAmount;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
