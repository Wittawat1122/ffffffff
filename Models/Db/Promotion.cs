using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? Title { get; set; }

    public int? DiscountPercent { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
