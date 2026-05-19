using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string Role { get; set; } = null!;

    public string? YearLevel { get; set; }

    public string AccountStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Conversation> ConversationBuyers { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationSellers { get; set; } = new List<Conversation>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> OrderBuyers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderSellers { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<PromotionOrder> PromotionOrders { get; set; } = new List<PromotionOrder>();

    public virtual ICollection<Rating> RatingReviewees { get; set; } = new List<Rating>();

    public virtual ICollection<Rating> RatingReviewers { get; set; } = new List<Rating>();

    public virtual ICollection<Report> ReportHandledByNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportReporters { get; set; } = new List<Report>();

    public virtual ICollection<Return> Returns { get; set; } = new List<Return>();

    public virtual ShipperProfile? ShipperProfile { get; set; }

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

    public virtual ICollection<UserVerification> UserVerificationReviewedByNavigations { get; set; } = new List<UserVerification>();

    public virtual ICollection<UserVerification> UserVerificationUsers { get; set; } = new List<UserVerification>();
}
