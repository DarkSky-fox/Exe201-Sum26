using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public int ProductId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Product Product { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;
}
