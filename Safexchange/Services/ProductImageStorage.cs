namespace Safexchange.Services;

public class ProductImageStorage : IProductImageStorage
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    private readonly string _productsPath;
    private readonly ILogger<ProductImageStorage> _logger;

    public ProductImageStorage(IWebHostEnvironment environment, ILogger<ProductImageStorage> logger)
    {
        _logger = logger;
        _productsPath = Path.Combine(environment.ContentRootPath, "Assets", "products");
        Directory.CreateDirectory(_productsPath);
    }

    public async Task<(bool Success, string? ImageUrl, string? Error)> SaveProductImageAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            return (false, null, "Vui lòng chọn file ảnh.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, null, "Ảnh không được vượt quá 5MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, null, "Chỉ chấp nhận ảnh JPG, PNG, GIF hoặc WEBP.");
        }

        if (!string.IsNullOrEmpty(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType))
        {
            return (false, null, "Định dạng ảnh không hợp lệ.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(_productsPath, fileName);

        try
        {
            await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
            await file.CopyToAsync(stream, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save product image {FileName}", fileName);
            return (false, null, "Không thể lưu ảnh. Vui lòng thử lại.");
        }

        return (true, $"/Assets/products/{fileName}", null);
    }

    public bool TryDeleteProductImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)
            || !imageUrl.StartsWith("/Assets/products/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var fullPath = Path.Combine(_productsPath, fileName);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            File.Delete(fullPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete product image {Path}", fullPath);
            return false;
        }
    }
}
