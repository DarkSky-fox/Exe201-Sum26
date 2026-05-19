using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int? OrderId { get; set; }

    public int ReviewerId { get; set; }

    public int RevieweeId { get; set; }

    public int RatingScore { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User Reviewee { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
