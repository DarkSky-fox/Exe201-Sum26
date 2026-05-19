using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int ReporterId { get; set; }

    public string TargetType { get; set; } = null!;

    public int TargetId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? HandledBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? HandledByNavigation { get; set; }

    public virtual ICollection<ReportImage> ReportImages { get; set; } = new List<ReportImage>();

    public virtual User Reporter { get; set; } = null!;
}
