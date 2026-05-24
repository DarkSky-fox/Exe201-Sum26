using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Products;

public class CreateModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateModel(SafexchangeDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [BindProperty]
    public ProductInputModel Input { get; set; } = new();

    public SelectList CategoryList { get; private set; } = null!;
    public SelectList StatusList { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!await _currentUser.IsUserVerifiedAsync())
        {
            return RedirectToPage("/Seller/Index");
        }

        await LoadSelectListsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!await _currentUser.IsUserVerifiedAsync())
        {
            return RedirectToPage("/Seller/Index");
        }

        await LoadSelectListsAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        var product = new Product
        {
            SellerId    = _currentUser.GetUserId(),
            CategoryId  = Input.CategoryId,
            StatusId    = Input.StatusId,
            Title       = Input.Title,
            Description = Input.Description,
            Price       = Input.Price,
            OriginalPrice  = Input.OriginalPrice,
            ConditionStatus = Input.ConditionStatus,
            ProductType    = Input.ProductType,
            IsNegotiable   = Input.IsNegotiable,
            CreatedAt      = DateTime.UtcNow,
            PublishedAt    = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        // Thêm ảnh bìa nếu có
        if (!string.IsNullOrWhiteSpace(Input.CoverImageUrl))
        {
            _db.ProductImages.Add(new ProductImage
            {
                ProductId = product.ProductId,
                ImageUrl  = Input.CoverImageUrl,
                IsCover   = true,
                SortOrder = 0
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
        return RedirectToPage("/Products/Index");
    }

    private async Task LoadSelectListsAsync(CancellationToken ct)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Select(c => new { c.CategoryId, c.CategoryName })
            .ToListAsync(ct);

        var statuses = await _db.ProductStatuses
            .AsNoTracking()
            .OrderBy(s => s.StatusId)
            .Select(s => new { s.StatusId, s.StatusName })
            .ToListAsync(ct);

        CategoryList = new SelectList(categories, "CategoryId", "CategoryName");
        StatusList   = new SelectList(statuses,   "StatusId",   "StatusName");
    }

    public class ProductInputModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public int StatusId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn tình trạng sản phẩm")]
        public string ConditionStatus { get; set; } = "like_new";

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public string ProductType { get; set; } = "normal";

        public bool IsNegotiable { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
