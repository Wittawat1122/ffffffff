using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = new List<LoyaltyPoint>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
