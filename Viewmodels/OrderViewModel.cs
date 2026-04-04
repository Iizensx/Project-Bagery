using System.Collections.Generic;

namespace _66022380.Models;

public class OrderRequest
{
    public int UserId { get; set; }
    public List<OrderItemRequest> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public int? AddressId { get; set; }
    public int? PromotionId { get; set; }
}

public class OrderItemRequest
{
    public int? ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
