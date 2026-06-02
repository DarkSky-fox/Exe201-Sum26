namespace Safexchange.Services;

public interface ICartAddService
{
    Task<CartAddResult> AddProductAsync(int buyerId, int productId, int quantity = 1);
}
