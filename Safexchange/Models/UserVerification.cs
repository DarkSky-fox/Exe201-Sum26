using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class UserVerification
{
    public int VerificationId { get; set; }

    public int UserId { get; set; }

    public string VerificationType { get; set; } = null!;

    public string? VerificationValue { get; set; }

    public string? DocumentUrl { get; set; }

    public string Status { get; set; } = null!;

    public int? ReviewedBy { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
