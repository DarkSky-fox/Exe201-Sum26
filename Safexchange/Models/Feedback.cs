using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int UserId { get; set; }

    public int? OrderId { get; set; }

    public string FeedbackType { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int? RatingScore { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
