using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Address
{
    public int AddressId { get; set; }

    public int? UserId { get; set; }

    public string? Address1 { get; set; }

    public string? Label { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
