using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Checkout;

public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICartService _cart;
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUser;

    public const string CodPaymentMethod = "Thanh toán khi nhận hàng";

    public IndexModel(
        SafexchangeDbContext db,
        ICartService cart,
        IOrderService orderService,
        ICurrentUserService currentUser)
    {
        _db = db;
        _cart = cart;
        _orderService = orderService;
        _currentUser = currentUser;
    }

    public CustomerInfo? Customer { get; private set; }
    public AddressInfo? DeliveryAddress { get; private set; }
    public IList<CheckoutLineItem> LineItems { get; private set; } = new List<CheckoutLineItem>();
    public decimal Subtotal { get; private set; }
    public decimal PlatformFees { get; private set; }
    public decimal GrandTotal { get; private set; }
    public int OrderCountPreview { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var items = _cart.GetItems();
        if (items.Count == 0)
        {
            TempData["CheckoutMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
            return RedirectToPage("/Products/Index");
        }

        await LoadPageDataAsync(items, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken cancellationToken)
    {
        var items = _cart.GetItems();
        if (items.Count == 0)
        {
            return RedirectToPage("/Products/Index");
        }

        var buyerId = _currentUser.GetUserId();
        var orders = await _orderService.CreateOrdersFromCartAsync(buyerId, items, cancellationToken);

        if (orders.Count == 0)
        {
            await LoadPageDataAsync(items, cancellationToken);
            ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng. Kiểm tra lại sản phẩm trong giỏ.");
            return Page();
        }

        _cart.Clear();
        TempData["OrderMessage"] = $"Đã đặt {orders.Count} đơn hàng thành công. Phương thức: {CodPaymentMethod}.";
        return RedirectToPage("/Orders/Index");
    }

    private async Task LoadPageDataAsync(IReadOnlyList<CartItem> items, CancellationToken cancellationToken)
    {
        var buyerId = _currentUser.GetUserId();

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == buyerId, cancellationToken);

        if (user is not null)
        {
            Customer = new CustomerInfo
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone ?? "—"
            };
        }

        var address = await _db.UserAddresses
            .AsNoTracking()
            .Include(a => a.Area)
            .Where(a => a.UserId == buyerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressId)
            .FirstOrDefaultAsync(cancellationToken);

        if (address is not null)
        {
            DeliveryAddress = new AddressInfo
            {
                ReceiverName = address.ReceiverName,
                Phone = address.Phone,
                AddressLine = address.AddressLine,
                AddressType = address.AddressType,
                AreaDisplay = FormatArea(address.Area)
            };
        }

        var lineItems = new List<CheckoutLineItem>();
        decimal subtotal = 0;
        decimal platformFees = 0;
        var orderCount = 0;

        foreach (var item in items)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId, cancellationToken);

            if (product is null || product.SellerId == buyerId)
            {
                continue;
            }

            var itemPrice = product.Price * item.Quantity;
            var fee = await _orderService.CalculatePlatformFeeAsync(itemPrice, cancellationToken);
            subtotal += itemPrice;
            platformFees += fee;
            orderCount++;

            lineItems.Add(new CheckoutLineItem
            {
                ProductId = item.ProductId,
                Title = item.Title,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineSubtotal = itemPrice,
                PlatformFee = fee,
                LineTotal = itemPrice + fee
            });
        }

        LineItems = lineItems;
        Subtotal = subtotal;
        PlatformFees = platformFees;
        GrandTotal = subtotal + platformFees;
        OrderCountPreview = orderCount;
    }

    private static string FormatArea(Area? area)
    {
        if (area is null)
        {
            return "—";
        }

        var parts = new[] { area.Ward, area.District, area.AreaName, area.City }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    public class CustomerInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class AddressInfo
    {
        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public string AreaDisplay { get; set; } = string.Empty;
    }

    public class CheckoutLineItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineSubtotal { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal LineTotal { get; set; }
    }
}
