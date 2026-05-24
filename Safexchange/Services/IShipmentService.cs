namespace Safexchange.Services;

public interface IShipmentService
{
    Task<int?> GetShipperIdForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<ShipperOrdersView> GetShipperOrdersAsync(int shipperId, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> AssignShipmentAsync(int shipmentId, int shipperId, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> AdvanceShipmentStatusAsync(int shipmentId, int shipperId, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> ConfirmCodPaymentAsync(int shipmentId, int shipperId, CancellationToken cancellationToken = default);
}

public class ShipperOrdersView
{
    public IReadOnlyList<ShipperOrderItem> AvailableOrders { get; init; } = Array.Empty<ShipperOrderItem>();

    public IReadOnlyList<ShipperOrderItem> MyOrders { get; init; } = Array.Empty<ShipperOrderItem>();
}

public class ShipperOrderItem
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public string ProductTitle { get; set; } = string.Empty;

    public string BuyerName { get; set; } = string.Empty;

    public string DeliveryAddress { get; set; } = string.Empty;

    public string DeliveryPhone { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string ShipStatusCode { get; set; } = string.Empty;

    public string ShipStatusName { get; set; } = string.Empty;

    public bool IsAssignedToMe { get; set; }

    public bool CanTakeOrder { get; set; }

    public bool CanStartDelivery { get; set; }

    public bool CanMarkDelivered { get; set; }

    public bool CanConfirmCod { get; set; }
}
