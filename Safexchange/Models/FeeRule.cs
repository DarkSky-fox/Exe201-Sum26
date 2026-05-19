using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class FeeRule
{
    public int FeeRuleId { get; set; }

    public string RuleName { get; set; } = null!;

    public decimal MinOrderValue { get; set; }

    public decimal? MaxOrderValue { get; set; }

    public string FeeType { get; set; } = null!;

    public decimal FeeValue { get; set; }

    public bool IsActive { get; set; }
}
