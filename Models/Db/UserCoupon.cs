using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class UserCoupon
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? CouponId { get; set; }

    public bool? IsUsed { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual User? User { get; set; }
}
