namespace Safexchange.Services;

public static class OrderDisplayHelper
{
    public static string OrderStatusLabel(string? status, string? shipStatusCode = null)
    {
        if (string.Equals(status, "confirmed", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(shipStatusCode))
        {
            return shipStatusCode.ToLowerInvariant() switch
            {
                "delivering" => "Đang giao hàng",
                "delivered" => "Đã giao — chờ thu tiền",
                "done" => "Hoàn tất",
                "waiting" => "Chờ shipper giao",
                _ => "Đã xác nhận"
            };
        }

        return (status ?? "").ToLowerInvariant() switch
        {
            "pending" => "Chờ xác nhận",
            "confirmed" => "Đã xác nhận",
            "completed" => "Hoàn tất",
            "cancelled" => "Đã hủy",
            "returned" => "Đã trả hàng",
            _ => status ?? "—"
        };
    }

    public static string PaymentStatusLabel(string? status) => (status ?? "").ToLowerInvariant() switch
    {
        "unpaid" => "Chưa thanh toán",
        "paid" => "Đã thanh toán",
        "refunded" => "Đã hoàn tiền",
        _ => status ?? "—"
    };

    public static string PaymentMethodLabel(string? method) => (method ?? "").ToLowerInvariant() switch
    {
        "cash" => "Thanh toán khi nhận hàng (COD)",
        "bank_transfer" => "Chuyển khoản",
        "momo" => "MoMo",
        "zalopay" => "ZaloPay",
        _ => method ?? "—"
    };

    public static string ShipStatusLabel(string? code, string? fallbackName = null)
    {
        if (!string.IsNullOrWhiteSpace(fallbackName))
        {
            return fallbackName;
        }

        return (code ?? "").ToLowerInvariant() switch
        {
            "waiting" => "Chờ giao hàng",
            "delivering" => "Đang giao hàng",
            "delivered" => "Đã giao — chờ thu tiền",
            "done" => "Hoàn tất giao hàng",
            _ => code ?? "—"
        };
    }

    public static string OrderStatusBadgeClass(string? status, string? shipStatusCode = null)
    {
        if (string.Equals(status, "confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return (shipStatusCode ?? "").ToLowerInvariant() switch
            {
                "delivering" => "bg-info text-dark",
                "delivered" => "bg-warning text-dark",
                "done" => "bg-success",
                _ => "bg-primary"
            };
        }

        return (status ?? "").ToLowerInvariant() switch
        {
            "completed" => "bg-success",
            "confirmed" => "bg-primary",
            "cancelled" or "returned" => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public static string PaymentStatusBadgeClass(string? status) => (status ?? "").ToLowerInvariant() switch
    {
        "paid" => "bg-success",
        "refunded" => "bg-warning text-dark",
        _ => "bg-warning text-dark"
    };
}
