namespace Safexchange.Services;

public class CheckoutInput
{
    public int BuyerId { get; set; }

    public IReadOnlyList<CartItem> Items { get; set; } = Array.Empty<CartItem>();

    public string ReceiverName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string? Note { get; set; }
}
