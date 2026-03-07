using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Finance.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("FINANCE_DB_CONNECTION") ??
            Environment.GetEnvironmentVariable("ConnectionStrings__FinanceDb") ??
            "Server=(localdb)\\mssqllocaldb;Database=FinanceDbDesignTime;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
