using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Orders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(
        SafexchangeDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public IList<OrderListItem> Orders { get; private set; }
        = new List<OrderListItem>();

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        var buyerId = _currentUser.GetUserId();

        Orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.BuyerId == buyerId)
            .Include(o => o.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItem
            {
                OrderId = o.OrderId,

                ProductTitle = o.Product.Title,

                ItemPrice = o.ItemPrice,

                PlatformFee = o.PlatformFee,

                ShippingFee = o.ShippingFee,

                DiscountAmount = o.DiscountAmount,

                TotalAmount = o.TotalAmount,

                OrderStatus = o.OrderStatus,

                PaymentStatus = o.PaymentStatus,

                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public class OrderListItem
    {
        public int OrderId { get; set; }

        public string ProductTitle { get; set; }
            = string.Empty;

        public decimal ItemPrice { get; set; }

        public decimal PlatformFee { get; set; }

        public decimal ShippingFee { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; }
            = string.Empty;

        public string PaymentStatus { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool CanEdit =>
            string.Equals(
                OrderStatus,
                "pending",
                StringComparison.OrdinalIgnoreCase);
    }
}