using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public int? AddressId { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual User? User { get; set; }
}
