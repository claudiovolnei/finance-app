using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Finance.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var basePath = Directory.GetCurrentDirectory();
        var apiProjectPath = Path.GetFullPath(Path.Combine(basePath, "../Finance.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiProjectPath) ? apiProjectPath : basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("FINANCE_DB_CONNECTION")
            ?? configuration.GetConnectionString("FinanceDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection not configured. Set FINANCE_DB_CONNECTION or ConnectionStrings:FinanceDb.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
