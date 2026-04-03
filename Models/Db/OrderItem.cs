using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? Size { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual Order? Order { get; set; }
}
