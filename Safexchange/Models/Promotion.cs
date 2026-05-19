using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionName { get; set; } = null!;

    public string PromotionType { get; set; } = null!;

    public int DurationDays { get; set; }

    public int MaxProducts { get; set; }

    public int MaxBoosts { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<PromotionOrder> PromotionOrders { get; set; } = new List<PromotionOrder>();
}
