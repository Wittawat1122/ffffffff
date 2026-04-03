using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class LoyaltyPoint
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? Points { get; set; }

    public virtual User? User { get; set; }
}
