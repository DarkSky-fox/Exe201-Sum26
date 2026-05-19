using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public string VoucherName { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal MinOrderValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int? UsageLimit { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
