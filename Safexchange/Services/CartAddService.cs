using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Services;

public class CartAddService : ICartAddService
{
    private readonly SafexchangeDbContext _db;
    private readonly ICartService _cart;

    public CartAddService(SafexchangeDbContext db, ICartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<CartAddResult> AddProductAsync(int buyerId, int productId, int quantity = 1)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.ProductImages)
            .Include(p => p.Status)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product is null)
        {
            return Fail("Sản phẩm không tồn tại.");
        }

        if (product.SellerId == buyerId)
        {
            return Fail("Bạn không thể mua sản phẩm của chính mình.");
        }

        if (!ProductStatusHelper.IsDisplayStatus(product.Status))
        {
            return Fail("Sản phẩm này đã ngừng hiển thị hoặc đã bán.");
        }

        var imageUrl = product.ProductImages
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();

        _cart.AddItem(new CartItem
        {
            ProductId = product.ProductId,
            SellerId = product.SellerId,
            Title = product.Title,
            ImageUrl = imageUrl,
            UnitPrice = product.Price,
            Quantity = Math.Max(1, quantity)
        });

        return new CartAddResult
        {
            Success = true,
            Message = "Đã thêm vào giỏ hàng.",
            CartCount = _cart.GetItemCount()
        };
    }

    private static CartAddResult Fail(string message) =>
        new() { Success = false, Message = message, CartCount = 0 };
}
