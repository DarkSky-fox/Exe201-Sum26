using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public int? ShipperId { get; set; }

    public int ShipMethodId { get; set; }

    public int ShipStatusId { get; set; }

    public int PickupAddressId { get; set; }

    public int DeliveryAddressId { get; set; }

    public decimal ShippingFee { get; set; }

    public string Payer { get; set; } = null!;

    public DateTime? ScheduledPickupAt { get; set; }

    public DateTime? PickedUpAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual UserAddress DeliveryAddress { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual UserAddress PickupAddress { get; set; } = null!;

    public virtual ShipMethod ShipMethod { get; set; } = null!;

    public virtual ShipStatus ShipStatus { get; set; } = null!;

    public virtual ShipperProfile? Shipper { get; set; }
}
