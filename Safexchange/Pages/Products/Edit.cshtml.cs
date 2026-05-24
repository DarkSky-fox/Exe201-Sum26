using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Products;

public class EditModel : PageModel
{
    private readonly SafexchangeDbContext _db;
    private readonly IProductImageStorage _imageStorage;

    public EditModel(SafexchangeDbContext db, IProductImageStorage imageStorage)
    {
        _db = db;
        _imageStorage = imageStorage;
    }

    [BindProperty]
    public ProductEditInput Input { get; set; } = new();

    [BindProperty]
    public IFormFile? CoverImage { get; set; }

    [BindProperty]
    public bool RemoveCoverImage { get; set; }

    public string? CurrentCoverImageUrl { get; private set; }

    public SelectList CategoryList { get; private set; } = null!;
    public SelectList StatusList { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product is null)
            return NotFound();

        await LoadProductInputAsync(id, cancellationToken);
        await LoadSelectListsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadSelectListsAsync(cancellationToken);
        await LoadCurrentCoverImageAsync(Input.ProductId, cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        var product = await _db.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == Input.ProductId, cancellationToken);

        if (product is null)
            return NotFound();

        product.Title           = Input.Title;
        product.Description     = Input.Description;
        product.Price           = Input.Price;
        product.OriginalPrice   = Input.OriginalPrice;
        product.CategoryId      = Input.CategoryId;
        product.StatusId        = Input.StatusId;
        product.ConditionStatus = Input.ConditionStatus;
        product.ProductType     = Input.ProductType;
        product.IsNegotiable    = Input.IsNegotiable;

        var coverImage = product.ProductImages
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        if (CoverImage is not null && CoverImage.Length > 0)
        {
            var imageResult = await _imageStorage.SaveProductImageAsync(CoverImage, cancellationToken);
            if (!imageResult.Success)
            {
                ModelState.AddModelError(nameof(CoverImage), imageResult.Error ?? "Không thể lưu ảnh.");
                return Page();
            }

            var oldUrl = coverImage?.ImageUrl;

            if (coverImage is not null)
            {
                coverImage.ImageUrl = imageResult.ImageUrl!;
                coverImage.IsCover  = true;
            }
            else
            {
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl  = imageResult.ImageUrl!,
                    IsCover   = true,
                    SortOrder = 0
                });
            }

            _imageStorage.TryDeleteProductImage(oldUrl);
            CurrentCoverImageUrl = imageResult.ImageUrl;
        }
        else if (RemoveCoverImage && coverImage is not null)
        {
            var oldUrl = coverImage.ImageUrl;
            _db.ProductImages.Remove(coverImage);
            _imageStorage.TryDeleteProductImage(oldUrl);
            CurrentCoverImageUrl = null;
        }

        await _db.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật!";
        return RedirectToPage("/Products/Index");
    }

    private async Task LoadProductInputAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

        if (product is null)
            return;

        Input = new ProductEditInput
        {
            ProductId       = product.ProductId,
            Title           = product.Title,
            Description     = product.Description,
            Price           = product.Price,
            OriginalPrice   = product.OriginalPrice,
            CategoryId      = product.CategoryId,
            StatusId        = product.StatusId,
            ConditionStatus = product.ConditionStatus,
            ProductType     = product.ProductType,
            IsNegotiable    = product.IsNegotiable
        };

        await LoadCurrentCoverImageAsync(productId, cancellationToken);
    }

    private async Task LoadCurrentCoverImageAsync(int productId, CancellationToken cancellationToken)
    {
        CurrentCoverImageUrl = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .OrderByDescending(i => i.IsCover)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefaultAsync(cancellationToken);
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
    }
}
