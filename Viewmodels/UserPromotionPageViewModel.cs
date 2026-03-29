using _66022380.Models.Db;

namespace _66022380.Models;

public class UserPromotionPageViewModel
{
    public List<Promotion> Promotions { get; set; } = new();
    public List<Promotion> EventPromotions { get; set; } = new();
    public Dictionary<int, string> RewardProductNames { get; set; } = new();
}
