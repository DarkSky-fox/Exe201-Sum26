using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Products;

public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;

    public IndexModel(SafexchangeDbContext db)
    {
        _db = db;
    }

    public IList<ProductListItem> Products { get; private set; } = new List<ProductListItem>();
    public IList<CategoryItem> Categories { get; private set; } = new List<CategoryItem>();
    public string? Search { get; private set; }
    public string? SelectedCategory { get; private set; }

    public async Task OnGetAsync(string? search, int? categoryId, string? category, CancellationToken cancellationToken)
    {
        Search = search;
        SelectedCategory = category;

        Categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryItem
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            })
            .ToListAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(SelectedCategory) && categoryId.HasValue)
        {
            SelectedCategory = Categories
                .FirstOrDefault(c => c.CategoryId == categoryId.Value)?
                .CategoryName;
        }

        var displayStatusIds = (await _db.ProductStatuses
                .AsNoTracking()
                .Select(s => new ProductStatus
                {
                    StatusId = s.StatusId,
                    StatusCode = s.StatusCode,
                    StatusName = s.StatusName
                })
                .ToListAsync(cancellationToken))
            .Where(ProductStatusHelper.IsDisplayStatus)
            .Select(s => s.StatusId)
            .ToList();

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Status)
            .Include(p => p.ProductImages)
            .Include(p => p.Seller)
            .AsQueryable();

        if (displayStatusIds.Count > 0)
        {
            query = query.Where(p => displayStatusIds.Contains(p.StatusId));
        }
        else
        {
            query = query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(SelectedCategory))
        {
            query = query.Where(p => p.Category.CategoryName == SelectedCategory);
        }
        else if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        Products = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
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

    public static string GetCategoryIcon(string categoryName) => categoryName switch
    {
        "Đồ điện tử" => "bi-laptop",
        "Đồ học tập" => "bi-pencil-square",
        "Sách/Giáo trình" => "bi-book",
        "Đồ phòng trọ" => "bi-house-door",
        "Combo phòng trọ" => "bi-box-seam",
        "Thời trang sinh viên" => "bi-bag-heart",
        _ => "bi-tag"
    };

    public class ProductListItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string ConditionStatus { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
    }

    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
