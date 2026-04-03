using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int? OrderId { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }
}
