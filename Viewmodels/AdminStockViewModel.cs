using _66022380.Models.Db;

namespace _66022380.Models;

public class AdminStockViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Stock> Products { get; set; } = new();
}
