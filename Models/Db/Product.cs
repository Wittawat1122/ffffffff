using System;
using System.Collections.Generic;

namespace fffff.Models.Db;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? FieldType { get; set; }

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
}
