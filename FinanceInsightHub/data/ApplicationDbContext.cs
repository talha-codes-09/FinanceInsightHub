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

        public DbSet<Transaction> Transactions { get; set; }
    }
}