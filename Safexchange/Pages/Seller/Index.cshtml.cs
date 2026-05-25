using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Seller;

[Authorize]
public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(SafexchangeDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public IList<ProductListItem> Products { get; private set; } = new List<ProductListItem>();
    public bool IsVerified { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IsVerified = await _currentUserService.IsUserVerifiedAsync();
        
        var currentUserId = _currentUserService.GetUserId();

        Products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProductImages)
            .Where(p => p.SellerId == currentUserId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductListItem
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                ConditionStatus = p.ConditionStatus,
                StatusName = p.Status.StatusName,
                CategoryName = p.Category.CategoryName,
                CoverImageUrl = p.ProductImages
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public class ProductListItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ConditionStatus { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
    }
}
