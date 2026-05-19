using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class PromotionOrder
{
    public int PromotionOrderId { get; set; }

    public int SellerId { get; set; }

    public int PromotionId { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual ICollection<PromotionList> PromotionLists { get; set; } = new List<PromotionList>();

    public virtual User Seller { get; set; } = null!;
}
