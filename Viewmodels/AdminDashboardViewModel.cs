namespace _66022380.Models;

public class AdminDashboardViewModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int PendingVerificationOrders { get; set; }
    public int TotalUsers { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int ActivePromotions { get; set; }
    public int CompletedOrders { get; set; }
    public int PreparingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public List<AdminDashboardMonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    public List<AdminDashboardRecentOrderItem> RecentOrders { get; set; } = new();
    public List<AdminDashboardLowStockItem> LowStockItems { get; set; } = new();
    public List<AdminDashboardCategoryItem> CategorySummaries { get; set; } = new();
}

public class AdminDashboardMonthlyRevenueItem
{
    public string Label { get; set; } = "";
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class AdminDashboardRecentOrderItem
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "-";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "-";
    public string PaymentStatus { get; set; } = "-";
    public string OrderDateText { get; set; } = "-";
}

public class AdminDashboardLowStockItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "-";
    public string CategoryName { get; set; } = "-";
    public int Stock { get; set; }
}

public class AdminDashboardCategoryItem
{
    public string CategoryName { get; set; } = "-";
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
}
