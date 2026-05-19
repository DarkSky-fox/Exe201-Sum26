using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Area
{
    public int AreaId { get; set; }

    public string AreaName { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? District { get; set; }

    public string? Ward { get; set; }

    public string AreaType { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<ShipperProfile> ShipperProfiles { get; set; } = new List<ShipperProfile>();

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}
