using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class PromotionList
{
    public int PromotionListId { get; set; }

    public int PromotionOrderId { get; set; }

    public int ProductId { get; set; }

    public string PromotionType { get; set; } = null!;

    public int PriorityScore { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual PromotionOrder PromotionOrder { get; set; } = null!;
}
