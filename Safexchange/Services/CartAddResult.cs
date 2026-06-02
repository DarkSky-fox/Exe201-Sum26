namespace Safexchange.Services;

public class CartAddResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CartCount { get; set; }
    public string? RedirectUrl { get; set; }
}
