using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceInsightHub.Data;

namespace FinanceInsightHub.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions.ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = totalIncome - totalExpenses;
            var count = transactions.Count;

            // This month vs last month, for trend arrows
            var now = DateTime.Now;
            var thisMonth = transactions.Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year);
            var lastMonthDate = now.AddMonths(-1);
            var lastMonth = transactions.Where(t => t.Date.Month == lastMonthDate.Month && t.Date.Year == lastMonthDate.Year);

            var thisMonthExpenses = thisMonth.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var lastMonthExpenses = lastMonth.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var expenseChangePercent = lastMonthExpenses == 0 ? 0 :
                Math.Round(((thisMonthExpenses - lastMonthExpenses) / lastMonthExpenses) * 100, 1);

            var thisMonthIncome = thisMonth.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var lastMonthIncome = lastMonth.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var incomeChangePercent = lastMonthIncome == 0 ? 0 :
                Math.Round(((thisMonthIncome - lastMonthIncome) / lastMonthIncome) * 100, 1);

            // Top spending category
            var topCategory = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();

            // Category breakdown for the progress bars
            var categoryBreakdown = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Percentage = totalExpenses == 0 ? 0 : Math.Round((g.Sum(t => t.Amount) / totalExpenses) * 100, 0)
                })
                .OrderByDescending(g => g.Total)
                .Take(5)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = balance,
                TransactionCount = count,
                IncomeChangePercent = incomeChangePercent,
                ExpenseChangePercent = expenseChangePercent,
                TopCategory = topCategory?.Category ?? "—",
                TopCategoryAmount = topCategory?.Total ?? 0,
                CategoryBreakdown = categoryBreakdown,
                RecentTransactions = transactions.OrderByDescending(t => t.Date).Take(6).ToList()
            };

            return View(model);
        }
    }
}