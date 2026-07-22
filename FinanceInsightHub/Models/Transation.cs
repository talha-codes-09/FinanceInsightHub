using System;
using System.ComponentModel.DataAnnotations;

namespace FinanceInsightHub.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000000)]
        public decimal Amount { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string TransactionType { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}