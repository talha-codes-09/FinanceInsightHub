using System.ComponentModel.DataAnnotations;

namespace FinanceInsightHub.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Goal name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target must be greater than 0")]
        public decimal TargetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Saved amount can't be negative")]
        public decimal SavedAmount { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? TargetDate { get; set; }

        public bool IsCompleted => SavedAmount >= TargetAmount;

        public decimal ProgressPercent => TargetAmount == 0 ? 0 : Math.Min(Math.Round((SavedAmount / TargetAmount) * 100, 0), 100);
    }
}