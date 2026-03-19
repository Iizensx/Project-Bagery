using _66022380.Models.Db;

namespace _66022380.Models;

public class AdminPromotionViewModel
{
    public List<Promotion> Promotions { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public Dictionary<int, int> PromotionUsageCounts { get; set; } = new();
    public Dictionary<int, int> PromotionGiftCounts { get; set; } = new();
}
