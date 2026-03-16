using Finance.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.Infrastructure.OpenFinance;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenFinanceIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PluggyOptions>(configuration.GetSection(PluggyOptions.SectionName));

        var baseUrl = configuration.GetValue<string>($"{PluggyOptions.SectionName}:BaseUrl") ?? "https://api.pluggy.ai";

        services.AddHttpClient("PluggyAuth", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient("PluggyApi", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<IPluggyAuthService, PluggyAuthService>();
        services.AddScoped<IPluggyService, PluggyService>();
        services.AddScoped<IOpenFinanceCategorizationService, OpenFinanceCategorizationService>();
        services.AddScoped<IPluggyConnectorResolver, PluggyConnectorResolver>();
        services.AddScoped<IPluggySyncService, PluggySyncService>();
        services.AddHostedService<PluggyTransactionSyncWorker>();

        return services;
    }
}
