using FinanceInsightHub.Models;

namespace FinanceInsightHub.Controllers
{
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public int TransactionCount { get; set; }
        public decimal IncomeChangePercent { get; set; }
        public decimal ExpenseChangePercent { get; set; }
        public string TopCategory { get; set; } = "—";
        public decimal TopCategoryAmount { get; set; }
        public List<CategorySummary> CategoryBreakdown { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();

        // Chart data
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyIncome { get; set; } = new();
        public List<decimal> MonthlyExpenses { get; set; } = new();
    }

    public class CategorySummary
    {
        public string Category { get; set; } = "";
        public decimal Total { get; set; }
        public decimal Percentage { get; set; }
    }
}