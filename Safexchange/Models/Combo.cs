using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Combo
{
    public int ComboId { get; set; }

    public int ProductId { get; set; }

    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    public string? ItemCondition { get; set; }

    public decimal? EstimatedValue { get; set; }

    public virtual Product Product { get; set; } = null!;
}
