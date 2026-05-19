using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Rule
{
    public int RuleId { get; set; }

    public string RuleName { get; set; } = null!;

    public string RuleType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
