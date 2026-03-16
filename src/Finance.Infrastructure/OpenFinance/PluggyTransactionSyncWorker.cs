using Finance.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggyTransactionSyncWorker(
    IPluggySyncService syncService,
    ILogger<PluggyTransactionSyncWorker> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Pluggy transaction sync worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await syncService.SyncCreditCardTransactionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Pluggy transaction sync cycle.");
            }

            await Task.Delay(SyncInterval, stoppingToken);
        }
    }
}
