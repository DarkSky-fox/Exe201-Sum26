using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ReportImage
{
    public int ReportImageId { get; set; }

    public int ReportId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Report Report { get; set; } = null!;
}
