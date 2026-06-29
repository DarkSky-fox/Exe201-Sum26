using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Pages.Shared;
using Safexchange.Services;

namespace Safexchange.Pages;

public class IndexModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public IndexModel(SafexchangeDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public IList<ProductListItem> Products { get; private set; } = new List<ProductListItem>();
    public IList<ProductListItem> VipProducts { get; private set; } = new List<ProductListItem>();
    public IList<ProductListItem> SponsoredProducts { get; private set; } = new List<ProductListItem>();
    public IList<CategoryItem> Categories { get; private set; } = new List<CategoryItem>();
    public HashSet<int> FavouriteIds { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var displayStatusIds = await GetDisplayStatusIdsAsync(cancellationToken);
        var userId = _currentUser.GetUserId();
        var isAuthenticated = User.Identity?.IsAuthenticated == true;

        // Get user's favourite product IDs
        if (isAuthenticated && userId != 0)
        {
            FavouriteIds = (await _db.Favourites
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId)
                .ToListAsync(cancellationToken))
                .ToHashSet();
        }

        Categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Take(6)
            .Select(c => new CategoryItem
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            })
            .ToListAsync(cancellationToken);

        Products = await LoadProductsAsync(
            _db.Products.AsNoTracking().Where(p => displayStatusIds.Contains(p.StatusId)),
            take: 12,
            cancellationToken);

        var promotedQuery = _db.PromotionLists
            .AsNoTracking()
            .Where(pl => pl.Status == "active"
                && pl.StartAt <= now
                && (pl.EndAt == null || pl.EndAt >= now)
                && pl.PromotionOrder.PaymentStatus == "paid");

        VipProducts = await LoadPromotedProductsAsync(
            promotedQuery.Where(pl =>
                pl.PromotionType == "combo_featured" || pl.PromotionType == "featured"),
            take: 6,
            displayStatusIds,
            cancellationToken);

        SponsoredProducts = await LoadPromotedProductsAsync(
            promotedQuery.Where(pl => pl.PromotionType == "boost"),
            take: 6,
            displayStatusIds,
            cancellationToken);
    }

    private async Task<List<ProductListItem>> LoadPromotedProductsAsync(
        IQueryable<PromotionList> query,
        int take,
        List<int> displayStatusIds,
        CancellationToken cancellationToken)
    {
        var productIds = await query
            .OrderByDescending(pl => pl.PriorityScore)
            .ThenByDescending(pl => pl.StartAt)
            .Select(pl => pl.ProductId)
            .Distinct()
            .Take(take)
            .ToListAsync(cancellationToken);

        if (productIds.Count == 0)
        {
            return new List<ProductListItem>();
        }

        var products = await LoadProductsAsync(
            _db.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId))
                .Where(p => displayStatusIds.Contains(p.StatusId)),
            take: productIds.Count,
            cancellationToken);

        return productIds
            .Select(id => products.FirstOrDefault(p => p.ProductId == id))
            .Where(p => p is not null)
            .Cast<ProductListItem>()
            .ToList();
    }

    private async Task<List<int>> GetDisplayStatusIdsAsync(CancellationToken cancellationToken)
    {
        var statuses = await _db.ProductStatuses
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return statuses
            .Where(ProductStatusHelper.IsDisplayStatus)
            .Select(s => s.StatusId)
            .ToList();
    }

    private async Task<List<ProductListItem>> LoadProductsAsync(
        IQueryable<Product> query,
        int take,
        CancellationToken cancellationToken)
    {
        return await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(take)
            .Select(p => new ProductListItem
            {
                ProductId = p.ProductId,
                Title = p.Title,
                Price = p.Price,
                ConditionStatus = p.ConditionStatus,
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

    public static string GetCategoryIcon(string categoryName) =>
        Safexchange.Pages.Products.IndexModel.GetCategoryIcon(categoryName);

    public static HomeProductCardViewModel ToCard(
        ProductListItem product,
        HomeProductCardVariant variant,
        bool isAuthenticated,
        bool showCartActions = false) =>
        HomeProductCardViewModel.FromListItem(
            product.ProductId,
            product.Title,
            product.Price,
            product.SellerName,
            product.CategoryName,
            product.ConditionStatus,
            product.CoverImageUrl,
            variant,
            isAuthenticated,
            showCartActions,
            isFavourited: false);

    public class ProductListItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ConditionStatus { get; set; } = string.Empty;
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
