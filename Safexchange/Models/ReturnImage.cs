using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ReturnImage
{
    public int ReturnImageId { get; set; }

    public int ReturnId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Return Return { get; set; } = null!;
}
