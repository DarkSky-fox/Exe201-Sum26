using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Return
{
    public int ReturnId { get; set; }

    public int OrderId { get; set; }

    public int RequesterId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Description { get; set; }

    public string ReturnStatus { get; set; } = null!;

    public decimal? RefundAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User Requester { get; set; } = null!;

    public virtual ICollection<ReturnImage> ReturnImages { get; set; } = new List<ReturnImage>();
}
