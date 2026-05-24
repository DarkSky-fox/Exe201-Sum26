namespace Safexchange.Services;

public class CheckoutResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<int> OrderIds { get; set; } = new();
}
