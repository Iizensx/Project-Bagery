using System;
using System.Collections.Generic;

namespace _66022380.Models.Db;

public partial class Address
{
    public int AddressId { get; set; }

    public int? UserId { get; set; }

    public string? AddressLine { get; set; }

    public string? District { get; set; }

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
