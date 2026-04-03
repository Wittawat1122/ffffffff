using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string? Code { get; set; }

    public int? DiscountPercent { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
