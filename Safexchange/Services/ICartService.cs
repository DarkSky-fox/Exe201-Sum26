namespace Safexchange.Services;

public interface ICartService
{
    IReadOnlyList<CartItem> GetItems();
    int GetItemCount();
    decimal GetTotal();
    void AddItem(CartItem item);
    void UpdateQuantity(int productId, int quantity);
    void RemoveItem(int productId);
    void Clear();
}
