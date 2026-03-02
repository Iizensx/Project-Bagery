using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}
