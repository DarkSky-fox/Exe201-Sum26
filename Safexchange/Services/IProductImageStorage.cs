namespace Safexchange.Services;

public interface IProductImageStorage
{
    Task<(bool Success, string? ImageUrl, string? Error)> SaveProductImageAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);

    bool TryDeleteProductImage(string? imageUrl);
}
