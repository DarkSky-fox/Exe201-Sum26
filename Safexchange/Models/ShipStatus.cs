using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class ShipStatus
{
    public int ShipStatusId { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
