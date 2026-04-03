using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class ProductSize
{
    public int SizeId { get; set; }

    public int? ProductId { get; set; }

    public int? Size { get; set; }

    public int? Stock { get; set; }

    public virtual Product? Product { get; set; }
}
