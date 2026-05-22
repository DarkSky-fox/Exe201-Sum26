using Safexchange.Models;

namespace Safexchange.Services;

public interface IShipmentService
{
    Task EnsureReferenceDataAsync(CancellationToken cancellationToken = default);
    Task<bool> TryCreateShipmentForOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShipperDeliveryItem>> GetAvailableDeliveriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShipperDeliveryItem>> GetMyActiveDeliveriesAsync(int shipperId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> AcceptDeliveryAsync(int shipmentId, int shipperId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> ConfirmCodCollectionAsync(int shipmentId, int shipperId, CancellationToken cancellationToken = default);
    Task<int?> GetShipperIdForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> ResolveShipperIdForCurrentUserAsync(int userId, CancellationToken cancellationToken = default);

    public class ShipperDeliveryItem
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ShipmentStatusCode { get; set; } = string.Empty;
        public string ShipmentStatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanAccept { get; set; }
        public bool CanConfirmCod { get; set; }
    }
}
