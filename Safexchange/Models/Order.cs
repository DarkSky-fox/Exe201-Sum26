using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public int ProductId { get; set; }

    public int? VoucherId { get; set; }

    public decimal ItemPrice { get; set; }

    public decimal PlatformFee { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Return> Returns { get; set; } = new List<Return>();

    public virtual User Seller { get; set; } = null!;

    public virtual Shipment? Shipment { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
