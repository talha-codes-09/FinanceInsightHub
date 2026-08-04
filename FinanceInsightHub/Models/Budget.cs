using System.ComponentModel.DataAnnotations;

namespace FinanceInsightHub.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Monthly limit is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal MonthlyLimit { get; set; }

        [Required]
        public int Month { get; set; } = DateTime.Now.Month;

        [Required]
        public int Year { get; set; } = DateTime.Now.Year;
    }
}