using System;
using System.Collections.Generic;

namespace Safexchange.Models;

public partial class Favourite
{
    public int FavouriteId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
