using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int? OrderId { get; set; }

    public int? PromotionOrderId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? TransactionCode { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual PromotionOrder? PromotionOrder { get; set; }

    public virtual User User { get; set; } = null!;
}
