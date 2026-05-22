using System.Text.Json;

namespace Safexchange.Services;

public class CartService : ICartService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public IReadOnlyList<CartItem> GetItems() => LoadCart();

    public IReadOnlyList<CartItem> GetItems(IEnumerable<int> productIds)
    {
        var idSet = productIds.ToHashSet();
        return LoadCart().Where(i => idSet.Contains(i.ProductId)).ToList();
    }

    public int GetItemCount() => LoadCart().Sum(i => i.Quantity);

    public decimal GetTotal() => LoadCart().Sum(i => i.LineTotal);

    public decimal GetSelectedTotal(IEnumerable<int> productIds)
        => GetItems(productIds).Sum(i => i.LineTotal);

    public void AddItem(CartItem item)
    {
        var cart = LoadCart();
        var existing = cart.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is not null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        SaveCart(cart);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var cart = LoadCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            cart.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        SaveCart(cart);
    }

    public void RemoveItem(int productId)
    {
        var cart = LoadCart();
        cart.RemoveAll(i => i.ProductId == productId);
        SaveCart(cart);
    }

    public void RemoveItems(IEnumerable<int> productIds)
    {
        var idSet = productIds.ToHashSet();
        var cart = LoadCart();
        cart.RemoveAll(i => idSet.Contains(i.ProductId));
        SaveCart(cart);
    }

    public void Clear()
    {
        Session.Remove(SessionKeys.Cart);
    }

    public void SetCheckoutProductIds(IEnumerable<int> productIds)
    {
        var ids = productIds.Distinct().ToList();
        Session.SetString(SessionKeys.CheckoutProductIds, JsonSerializer.Serialize(ids, JsonOptions));
    }

    public IReadOnlyList<int> GetCheckoutProductIds()
    {
        var json = Session.GetString(SessionKeys.CheckoutProductIds);
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<int>();
        }

        return JsonSerializer.Deserialize<List<int>>(json, JsonOptions) ?? new List<int>();
    }

    public void ClearCheckoutProductIds()
    {
        Session.Remove(SessionKeys.CheckoutProductIds);
    }

    private List<CartItem> LoadCart()
    {
        var json = Session.GetString(SessionKeys.Cart);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json, JsonOptions) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        Session.SetString(SessionKeys.Cart, JsonSerializer.Serialize(cart, JsonOptions));
    }
}
