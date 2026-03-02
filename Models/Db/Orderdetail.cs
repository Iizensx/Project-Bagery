using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Orderdetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Stock? Product { get; set; }
}
