using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Pages.Products;

public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;

    public IndexModel(SafexchangeDbContext db)
    {
        _db = db;
    }

    public IList<ProductListItem> Products { get; set; }
        = new List<ProductListItem>();

    public async Task OnGetAsync(
        string? search,
        int? categoryId,
        string? category,
        CancellationToken cancellationToken)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProductImages)
            .Include(p => p.Seller)
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Title.Contains(search));
        }

        // FILTER CATEGORY ID
        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        // FILTER CATEGORY NAME
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p =>
                p.Category.CategoryName == category);
        }

        Products = await query
            .OrderByDescending(p =>
                p.PublishedAt ?? p.CreatedAt)
            .Select(p => new ProductListItem
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                ConditionStatus = p.ConditionStatus,
                StatusName = p.Status.StatusName,
                CategoryName = p.Category.CategoryName,
                SellerName = p.Seller.FullName,

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

        public string Title { get; set; }
            = string.Empty;

        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        public string ConditionStatus { get; set; }
            = string.Empty;

        public string StatusName { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;

        public string SellerName { get; set; }
            = string.Empty;

        public string? CoverImageUrl { get; set; }
    }
}