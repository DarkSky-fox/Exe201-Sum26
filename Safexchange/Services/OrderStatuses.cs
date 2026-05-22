namespace Safexchange.Services;

public static class OrderStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
    public const string Returned = "returned";

    public static string ToDisplay(string status, string? shipmentStatusCode = null)
    {
        if (string.Equals(status, Confirmed, StringComparison.OrdinalIgnoreCase)
            && string.Equals(shipmentStatusCode, ShipmentStatusCodes.InTransit, StringComparison.OrdinalIgnoreCase))
        {
            return "Đang giao hàng";
        }

        return status.ToLowerInvariant() switch
        {
            Pending when shipmentStatusCode == ShipmentStatusCodes.PendingPickup => "Chờ shipper lấy hàng",
            Pending => "Chờ xử lý",
            Confirmed => "Đã xác nhận",
            Completed => "Hoàn thành",
            Cancelled => "Đã hủy",
            Returned => "Đã trả hàng",
            _ => status
        };
    }
}

public static class PaymentStatuses
{
    public const string Unpaid = "unpaid";
    public const string Paid = "paid";
    public const string Refunded = "refunded";

    public static string ToDisplay(string status) => status.ToLowerInvariant() switch
    {
        Unpaid => "Chưa thanh toán",
        Paid => "Đã thanh toán",
        Refunded => "Đã hoàn tiền",
        _ => status
    };

    public static string BadgeClass(string status) => status.ToLowerInvariant() switch
    {
        Paid => "bg-success",
        Refunded => "bg-info text-dark",
        _ => "bg-warning text-dark"
    };
}

public static class ShipmentStatusCodes
{
    public const string PendingPickup = "pending_pickup";
    public const string InTransit = "in_transit";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";

    public static string ToDisplay(string? code) => (code ?? "").ToLowerInvariant() switch
    {
        PendingPickup => "Chờ lấy hàng",
        InTransit => "Đang giao",
        Delivered => "Đã giao",
        Cancelled => "Đã hủy",
        _ => "—"
    };
}
