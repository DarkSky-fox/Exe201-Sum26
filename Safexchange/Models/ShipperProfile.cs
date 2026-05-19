using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ShipperProfile
{
    public int ShipperId { get; set; }

    public int UserId { get; set; }

    public int? AreaId { get; set; }

    public string VehicleType { get; set; } = null!;

    public string? LicensePlate { get; set; }

    public string ShipperStatus { get; set; } = null!;

    public decimal RatingAvg { get; set; }

    public virtual Area? Area { get; set; }

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual User User { get; set; } = null!;
}
