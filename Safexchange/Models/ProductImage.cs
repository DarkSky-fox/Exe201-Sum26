using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsCover { get; set; }

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
