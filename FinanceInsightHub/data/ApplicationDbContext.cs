using Microsoft.EntityFrameworkCore;
using FinanceInsightHub.Models;

namespace FinanceInsightHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Transaction table
        public DbSet<Transaction> Transactions { get; set; }
    }
}