using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;

namespace Finance.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.TransferAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}