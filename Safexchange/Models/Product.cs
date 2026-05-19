using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public int StatusId { get; set; }

    public int? AreaId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public string ConditionStatus { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public bool IsNegotiable { get; set; }

    public int ViewCount { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Area? Area { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Combo> Combos { get; set; } = new List<Combo>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<PromotionList> PromotionLists { get; set; } = new List<PromotionList>();

    public virtual User Seller { get; set; } = null!;

    public virtual ProductStatus Status { get; set; } = null!;
}
