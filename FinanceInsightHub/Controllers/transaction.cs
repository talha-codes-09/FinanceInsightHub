using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceInsightHub.Data;
using FinanceInsightHub.Models;

namespace FinanceInsightHub.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(string searchTitle, string category, string type, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                query = query.Where(t => t.Title.Contains(searchTitle));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => t.Type == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();

            ViewBag.SearchTitle = searchTitle;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(transactions);
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Amount,Date,Category,Type")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Amount,Date,Category,Type")] Transaction transaction)
        {
            if (id != transaction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}