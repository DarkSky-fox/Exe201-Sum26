using Safexchange.Models;

namespace Safexchange.Services;

public interface IOrderService
{
    Task<decimal> CalculatePlatformFeeAsync(decimal itemPrice, CancellationToken cancellationToken = default);
    Task<decimal> CalculateDiscountAsync(Voucher? voucher, decimal itemPrice, CancellationToken cancellationToken = default);
    Task<List<Order>> CreateOrdersFromCartAsync(int buyerId, IReadOnlyList<CartItem> items, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderForBuyerAsync(int orderId, int buyerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderAsync(int orderId, int buyerId, decimal shippingFee, string? voucherCode, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderForSellerByProductIdAsync(int productId, int sellerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(int orderId, int sellerId, string newStatus, CancellationToken cancellationToken = default);
}
