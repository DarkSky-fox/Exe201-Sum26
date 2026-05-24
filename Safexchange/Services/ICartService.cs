namespace Safexchange.Services;

public interface ICartService
{
    IReadOnlyList<CartItem> GetItems();
    IReadOnlyList<CartItem> GetItems(IEnumerable<int> productIds);
    int GetItemCount();
    decimal GetTotal();
    decimal GetSelectedTotal(IEnumerable<int> productIds);
    void AddItem(CartItem item);
    void UpdateQuantity(int productId, int quantity);
    void RemoveItem(int productId);
    void RemoveItems(IEnumerable<int> productIds);
    void Clear();
    void SetCheckoutProductIds(IEnumerable<int> productIds);
    IReadOnlyList<int> GetCheckoutProductIds();
    void ClearCheckoutProductIds();
}
