using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;
using Safexchange.Validation;
using System.ComponentModel.DataAnnotations;

namespace Safexchange.Pages.Checkout;

[Authorize]
public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICartService _cart;
    private readonly ICheckoutService _checkout;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(
        SafexchangeDbContext db,
        ICartService cart,
        ICheckoutService checkout,
        ICurrentUserService currentUser)
    {
        _db = db;
        _cart = cart;
        _checkout = checkout;
        _currentUser = currentUser;
    }

    public IReadOnlyList<CartItem> Items { get; private set; } = Array.Empty<CartItem>();

    public decimal Subtotal { get; private set; }

    public decimal EstimatedShipping { get; private set; }

    public decimal EstimatedTotal { get; private set; }

    [BindProperty]
    public CheckoutForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!await LoadCheckoutAsync(cancellationToken))
        {
            return RedirectToPage("/Products/Index");
        }

        await PrefillBuyerInfoAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!await LoadCheckoutAsync(cancellationToken))
        {
            return RedirectToPage("/Products/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var normalizedPhone = VietnamesePhoneAttribute.Normalize(Form.Phone);
        if (normalizedPhone is null)
        {
            ModelState.AddModelError(nameof(Form.Phone), "Số điện thoại không hợp lệ. Vui lòng nhập số Việt Nam (vd: 0912345678).");
            return Page();
        }

        Form.Phone = normalizedPhone;

        var buyerId = _currentUser.GetUserId();
        var result = await _checkout.PlaceOrderAsync(new CheckoutInput
        {
            BuyerId = buyerId,
            Items = Items,
            ReceiverName = Form.ReceiverName,
            Phone = Form.Phone,
            AddressLine = Form.AddressLine,
            Note = Form.Note
        }, cancellationToken);

        if (!result.Success)
        {
            if (result.UnavailableProductIds.Count > 0)
            {
                _cart.RemoveItems(result.UnavailableProductIds);
                _cart.ClearCheckoutProductIds();
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        var productIds = result.OrderIds.Any()
            ? await _db.Orders
                .AsNoTracking()
                .Where(o => result.OrderIds.Contains(o.OrderId))
                .Select(o => o.ProductId)
                .Distinct()
                .ToListAsync(cancellationToken)
            : Items.Select(i => i.ProductId).ToList();

        if (result.UnavailableProductIds.Count > 0)
        {
            productIds.AddRange(result.UnavailableProductIds);
        }

        _cart.RemoveItems(productIds);
        _cart.ClearCheckoutProductIds();

        TempData["OrderMessage"] = result.Message;
        return RedirectToPage("/Orders/Index");
    }

    private async Task<bool> LoadCheckoutAsync(CancellationToken cancellationToken)
    {
        var productIds = _cart.GetCheckoutProductIds();
        if (productIds.Count == 0)
        {
            return false;
        }

        Items = _cart.GetItems(productIds);
        if (Items.Count == 0)
        {
            return false;
        }

        Subtotal = Items.Sum(i => i.LineTotal);

        await _checkout.EnsureReferenceDataAsync(cancellationToken);
        var shipMethod = await _db.ShipMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.ShipMethodId)
            .FirstOrDefaultAsync(cancellationToken);

        EstimatedShipping = (shipMethod?.BaseFee ?? 20000) * Items.Count;
        EstimatedTotal = Subtotal + EstimatedShipping;
        return true;
    }

    private async Task PrefillBuyerInfoAsync(CancellationToken cancellationToken)
    {
        var buyerId = _currentUser.GetUserId();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == buyerId, cancellationToken);
        var defaultAddress = await _db.UserAddresses
            .AsNoTracking()
            .Where(a => a.UserId == buyerId && a.AddressType == "delivery")
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.AddressId)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultAddress is not null)
        {
            Form.ReceiverName = defaultAddress.ReceiverName;
            Form.Phone = defaultAddress.Phone;
            Form.AddressLine = defaultAddress.AddressLine;
        }
        else if (user is not null)
        {
            Form.ReceiverName = user.FullName;
            Form.Phone = user.Phone ?? string.Empty;
        }
    }

    public class CheckoutForm
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [Display(Name = "Họ và tên người nhận")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [VietnamesePhone]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string AddressLine { get; set; } = string.Empty;

        [Display(Name = "Ghi chú đơn hàng")]
        public string? Note { get; set; }
    }
}
