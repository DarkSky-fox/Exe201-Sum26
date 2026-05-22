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

    public IList<OrderListItem> Orders { get; private set; } = new List<OrderListItem>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var buyerId = _currentUser.GetUserId();

        Orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.BuyerId == buyerId)
            .Include(o => o.Product)
            .Include(o => o.Shipment!)
                .ThenInclude(s => s.ShipStatus)
            .Include(o => o.Shipment!)
                .ThenInclude(s => s.DeliveryAddress)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItem
            {
                OrderId = o.OrderId,
                ProductTitle = o.Product.Title,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.Payments
                    .OrderByDescending(p => p.PaymentId)
                    .Select(p => p.PaymentMethod)
                    .FirstOrDefault() ?? "cash",
                ShipStatusCode = o.Shipment != null ? o.Shipment.ShipStatus.StatusCode : null,
                ShipStatusName = o.Shipment != null ? o.Shipment.ShipStatus.StatusName : null,
                DeliveryAddress = o.Shipment != null ? o.Shipment.DeliveryAddress.AddressLine : null,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public class OrderListItem
    {
        public int OrderId { get; set; }

        public string ProductTitle { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "cash";

        public string? ShipStatusCode { get; set; }

        public string? ShipStatusName { get; set; }

        public string? DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
