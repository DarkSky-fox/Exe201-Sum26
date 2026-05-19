using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class UserAddress
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public int? AreaId { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string AddressType { get; set; } = null!;

    public bool IsDefault { get; set; }

    public virtual Area? Area { get; set; }

    public virtual ICollection<Shipment> ShipmentDeliveryAddresses { get; set; } = new List<Shipment>();

    public virtual ICollection<Shipment> ShipmentPickupAddresses { get; set; } = new List<Shipment>();

    public virtual User User { get; set; } = null!;
}
