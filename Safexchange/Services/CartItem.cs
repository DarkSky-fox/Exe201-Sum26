namespace Safexchange.Services;

public class CartItem
{
    public int ProductId { get; set; }
    public int SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal LineTotal => UnitPrice * Quantity;
}
