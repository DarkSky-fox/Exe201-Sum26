using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;

namespace Safexchange.Pages.Products;

public class EditModel : PageModel
{
    private readonly SafexchangeDbContext _db;

    public EditModel(SafexchangeDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ProductEditInput Input { get; set; } = new();

    public SelectList CategoryList { get; private set; } = null!;
    public SelectList StatusList { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product is null)
            return NotFound();

        Input = new ProductEditInput
        {
            ProductId      = product.ProductId,
            Title          = product.Title,
            Description    = product.Description,
            Price          = product.Price,
            OriginalPrice  = product.OriginalPrice,
            CategoryId     = product.CategoryId,
            StatusId       = product.StatusId,
            ConditionStatus = product.ConditionStatus,
            ProductType    = product.ProductType,
            IsNegotiable   = product.IsNegotiable,
            CoverImageUrl  = product.ProductImages
                .OrderByDescending(i => i.IsCover)
                .ThenBy(i => i.SortOrder)
                .Select(i => i.ImageUrl)
                .FirstOrDefault()
        };

        await LoadSelectListsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadSelectListsAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        var product = await _db.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == Input.ProductId, cancellationToken);

        if (product is null)
            return NotFound();

        // Cập nhật các trường
        product.Title           = Input.Title;
        product.Description     = Input.Description;
        product.Price           = Input.Price;
        product.OriginalPrice   = Input.OriginalPrice;
        product.CategoryId      = Input.CategoryId;
        product.StatusId        = Input.StatusId;
        product.ConditionStatus = Input.ConditionStatus;
        product.ProductType     = Input.ProductType;
        product.IsNegotiable    = Input.IsNegotiable;

        // Cập nhật ảnh bìa
        var coverImage = product.ProductImages
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(Input.CoverImageUrl))
        {
            if (coverImage is not null)
            {
                coverImage.ImageUrl = Input.CoverImageUrl;
                coverImage.IsCover  = true;
            }
            else
            {
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl  = Input.CoverImageUrl,
                    IsCover   = true,
                    SortOrder = 0
                });
            }
        }
        else if (coverImage is not null)
        {
            _db.ProductImages.Remove(coverImage);
        }

        await _db.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật!";
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

    public class ProductEditInput
    {
        public int ProductId { get; set; }

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
        public string ProductType { get; set; } = "single";

        public bool IsNegotiable { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}
