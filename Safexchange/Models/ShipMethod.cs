using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ShipMethod
{
    public int ShipMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal BaseFee { get; set; }

    public string? EstimatedTime { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
