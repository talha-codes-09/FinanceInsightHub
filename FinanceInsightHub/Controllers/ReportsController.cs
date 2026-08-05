using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceInsightHub.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using System.Text;

namespace FinanceInsightHub.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var model = await BuildReport(month, year);
            return View(model);
        }

        public async Task<IActionResult> ExportCsv(int? month, int? year)
        {
            var model = await BuildReport(month, year);
            var sb = new StringBuilder();
            sb.AppendLine("Title,Category,Type,Date,Amount");

            foreach (var t in model.IncomeTransactions.Concat(model.ExpenseTransactions).OrderBy(t => t.Date))
            {
                sb.AppendLine($"\"{t.Title}\",\"{t.Category}\",\"{t.Type}\",{t.Date:yyyy-MM-dd},{t.Amount}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Report_{model.Month:00}_{model.Year}.csv";
            return File(bytes, "text/csv", fileName);
        }

        public async Task<IActionResult> ExportExcel(int? month, int? year)
        {
            var model = await BuildReport(month, year);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Report");

            ws.Cell(1, 1).Value = "Title";
            ws.Cell(1, 2).Value = "Category";
            ws.Cell(1, 3).Value = "Type";
            ws.Cell(1, 4).Value = "Date";
            ws.Cell(1, 5).Value = "Amount";
            ws.Range(1, 1, 1, 5).Style.Font.Bold = true;

            int row = 2;
            foreach (var t in model.IncomeTransactions.Concat(model.ExpenseTransactions).OrderBy(t => t.Date))
            {
                ws.Cell(row, 1).Value = t.Title;
                ws.Cell(row, 2).Value = t.Category;
                ws.Cell(row, 3).Value = t.Type;
                ws.Cell(row, 4).Value = t.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 5).Value = t.Amount;
                row++;
            }

            ws.Cell(row + 1, 4).Value = "Total Income";
            ws.Cell(row + 1, 5).Value = model.TotalIncome;
            ws.Cell(row + 2, 4).Value = "Total Expenses";
            ws.Cell(row + 2, 5).Value = model.TotalExpenses;
            ws.Cell(row + 3, 4).Value = "Net Savings";
            ws.Cell(row + 3, 5).Value = model.NetSavings;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"Report_{model.Month:00}_{model.Year}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> ExportPdf(int? month, int? year)
        {
            var model = await BuildReport(month, year);
            var monthName = new DateTime(model.Year, model.Month, 1).ToString("MMMM yyyy");

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text($"Finance Insight Hub — Report ({monthName})")
                        .FontSize(18).Bold();

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Total Income: {model.TotalIncome:C}").FontSize(12);
                        col.Item().Text($"Total Expenses: {model.TotalExpenses:C}").FontSize(12);
                        col.Item().Text($"Net Savings: {model.NetSavings:C}").FontSize(12).Bold();

                        col.Item().PaddingTop(15).Text("Category-wise Expense Summary").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Category").Bold();
                                h.Cell().Text("Count").Bold();
                                h.Cell().Text("Total").Bold();
                            });

                            foreach (var c in model.CategorySummary)
                            {
                                table.Cell().Text(c.Category);
                                table.Cell().Text(c.Count.ToString());
                                table.Cell().Text(c.Total.ToString("C"));
                            }
                        });

                        col.Item().PaddingTop(15).Text("All Transactions").FontSize(13).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Title").Bold();
                                h.Cell().Text("Category").Bold();
                                h.Cell().Text("Type").Bold();
                                h.Cell().Text("Date").Bold();
                                h.Cell().Text("Amount").Bold();
                            });

                            foreach (var t in model.IncomeTransactions.Concat(model.ExpenseTransactions).OrderBy(t => t.Date))
                            {
                                table.Cell().Text(t.Title);
                                table.Cell().Text(t.Category);
                                table.Cell().Text(t.Type);
                                table.Cell().Text(t.Date.ToString("MMM dd"));
                                table.Cell().Text(t.Amount.ToString("C"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated by Finance Insight Hub").FontSize(8);
                    });
                });
            });

            var bytes = document.GeneratePdf();
            var fileName = $"Report_{model.Month:00}_{model.Year}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        private async Task<ReportViewModel> BuildReport(int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedMonth = month ?? now.Month;
            int selectedYear = year ?? now.Year;

            var transactions = await _context.Transactions
                .Where(t => t.Date.Month == selectedMonth && t.Date.Year == selectedYear)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            var categorySummary = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new ReportCategoryRow
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            return new ReportViewModel
            {
                Month = selectedMonth,
                Year = selectedYear,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetSavings = totalIncome - totalExpenses,
                IncomeTransactions = transactions.Where(t => t.Type == "Income").OrderByDescending(t => t.Date).ToList(),
                ExpenseTransactions = transactions.Where(t => t.Type == "Expense").OrderByDescending(t => t.Date).ToList(),
                CategorySummary = categorySummary
            };
        }
    }

    public class ReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings { get; set; }
        public List<FinanceInsightHub.Models.Transaction> IncomeTransactions { get; set; } = new();
        public List<FinanceInsightHub.Models.Transaction> ExpenseTransactions { get; set; } = new();
        public List<ReportCategoryRow> CategorySummary { get; set; } = new();
    }

    public class ReportCategoryRow
    {
        public string Category { get; set; } = "";
        public decimal Total { get; set; }
        public int Count { get; set; }
    }
}