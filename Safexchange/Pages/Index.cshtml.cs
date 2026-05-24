using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SafexchangeDbContext _context;

        public IndexModel(SafexchangeDbContext context)
        {
            _context = context;
        }

        public IList<ProductViewModel> Products { get; set; }
            = new List<ProductViewModel>();

        public IList<ProductViewModel> VipProducts { get; set; }
            = new List<ProductViewModel>();

        public IList<ProductViewModel> SponsoredProducts { get; set; }
            = new List<ProductViewModel>();

        public async Task OnGetAsync()
        {
            var now = DateTime.Now;

            // VIP PRODUCTS
            VipProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.PromotionLists)
                .Where(p => p.PromotionLists.Any(pl =>
                    pl.Status == "active" &&
                    pl.PriorityScore >= 100 &&
                    (pl.EndAt == null || pl.EndAt > now)))
                .OrderByDescending(p =>
                    p.PromotionLists.Max(pl => pl.PriorityScore))
                .Take(6)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Price = p.Price,

                    CoverImageUrl = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    CategoryName = p.Category.CategoryName,
                    SellerName = p.Seller.FullName,
                    ConditionStatus = p.ConditionStatus
                })
                .ToListAsync();

            // SPONSORED PRODUCTS
            SponsoredProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Include(p => p.PromotionLists)
                .Where(p => p.PromotionLists.Any(pl =>
                    pl.Status == "active" &&
                    (pl.EndAt == null || pl.EndAt > now)))
                .OrderByDescending(p =>
                    p.PromotionLists.Max(pl => pl.PriorityScore))
                .ThenByDescending(p => p.CreatedAt)
                .Take(12)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Price = p.Price,

                    CoverImageUrl = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    CategoryName = p.Category.CategoryName,
                    SellerName = p.Seller.FullName,
                    ConditionStatus = p.ConditionStatus
                })
                .ToListAsync();

            // NORMAL PRODUCTS
            Products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Price = p.Price,

                    CoverImageUrl = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    CategoryName = p.Category.CategoryName,
                    SellerName = p.Seller.FullName,
                    ConditionStatus = p.ConditionStatus
                })
                .ToListAsync();
        }

        public class ProductViewModel
        {
            public int ProductId { get; set; }

            public string Title { get; set; }
                = string.Empty;

            public decimal Price { get; set; }

            public string? CoverImageUrl { get; set; }

            public string CategoryName { get; set; }
                = string.Empty;

            public string SellerName { get; set; }
                = string.Empty;

            public string ConditionStatus { get; set; }
                = string.Empty;
        }
    }
}