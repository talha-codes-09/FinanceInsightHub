using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceInsightHub.Data;
using FinanceInsightHub.Models;

namespace FinanceInsightHub.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Budgets
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var budgets = await _context.Budgets
                .Where(b => b.Month == now.Month && b.Year == now.Year)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.Type == "Expense" && t.Date.Month == now.Month && t.Date.Year == now.Year)
                .ToListAsync();

            var viewModels = budgets.Select(b =>
            {
                var spent = transactions.Where(t => t.Category == b.Category).Sum(t => t.Amount);
                var remaining = b.MonthlyLimit - spent;
                var percentUsed = b.MonthlyLimit == 0 ? 0 : Math.Round((spent / b.MonthlyLimit) * 100, 0);

                return new BudgetViewModel
                {
                    Id = b.Id,
                    Category = b.Category,
                    MonthlyLimit = b.MonthlyLimit,
                    Spent = spent,
                    Remaining = remaining,
                    PercentUsed = percentUsed
                };
            }).ToList();

            return View(viewModels);
        }

        // GET: Budgets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Category,MonthlyLimit")] Budget budget)
        {
            budget.Month = DateTime.Now.Month;
            budget.Year = DateTime.Now.Year;

            if (ModelState.IsValid)
            {
                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }

        // GET: Budgets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        // POST: Budgets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Category,MonthlyLimit,Month,Year")] Budget budget)
        {
            if (id != budget.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class BudgetViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentUsed { get; set; }
    }
}