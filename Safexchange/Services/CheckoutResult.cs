namespace Safexchange.Services;

public class CheckoutResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<int> OrderIds { get; set; } = new();

    public List<int> UnavailableProductIds { get; set; } = new();

    public List<string> UnavailableProductTitles { get; set; } = new();
}
