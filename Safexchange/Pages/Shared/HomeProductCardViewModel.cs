namespace Safexchange.Pages.Shared;

public class HomeProductCardViewModel
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? ConditionStatus { get; set; }
    public string? CoverImageUrl { get; set; }
    public HomeProductCardVariant Variant { get; set; } = HomeProductCardVariant.Featured;
    public bool ShowCartActions { get; set; }
    public bool CartIconOnly { get; set; }
    public bool IsAuthenticated { get; set; }

    public static HomeProductCardViewModel FromListItem(
        int productId,
        string title,
        decimal price,
        string sellerName,
        string? categoryName,
        string? conditionStatus,
        string? coverImageUrl,
        HomeProductCardVariant variant,
        bool isAuthenticated,
        bool showCartActions = false,
        bool cartIconOnly = false) =>
        new()
        {
            ProductId = productId,
            Title = title,
            Price = price,
            SellerName = sellerName,
            CategoryName = categoryName,
            ConditionStatus = conditionStatus,
            CoverImageUrl = coverImageUrl,
            Variant = variant,
            IsAuthenticated = isAuthenticated,
            ShowCartActions = showCartActions,
            CartIconOnly = cartIconOnly
        };

    public string? ResolvedImageUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CoverImageUrl))
            {
                return null;
            }

            var url = CoverImageUrl.Trim();
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith('/'))
            {
                return url;
            }

            return "/" + url.TrimStart('/');
        }
    }

    public string ConditionLabel => ConditionStatus switch
    {
        "like_new" => "Như mới",
        "good" => "Tốt",
        "fair" => "Khá",
        "need_repair" => "Cần sửa",
        _ => ConditionStatus ?? string.Empty
    };
}

public enum HomeProductCardVariant
{
    Vip,
    Sponsored,
    Featured,
    Catalog
}
