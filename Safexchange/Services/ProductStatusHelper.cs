using Safexchange.Models;

namespace Safexchange.Services;

public static class ProductStatusHelper
{
    public static bool IsDisplayStatus(ProductStatus status)
    {
        var code = Normalize(status.StatusCode);
        var name = Normalize(status.StatusName);

        return code is "available" or "displaying" or "active" or "published" or "showing"
            || name.Contains("danghienthi")
            || name.Contains("hienthi")
            || name.Contains("available");
    }

    public static bool IsSoldStatus(ProductStatus status)
    {
        var code = Normalize(status.StatusCode);
        var name = Normalize(status.StatusName);

        return code is "sold" or "soldout"
            || name.Contains("daban")
            || name.Contains("sold");
    }

    private static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var value = input.Trim().ToLowerInvariant()
            .Replace('đ', 'd');

        return RemoveDiacritics(value)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var filtered = normalized.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
            != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(filtered.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
    }
}
