using Finance.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggySyncService(
    IPluggyService pluggyService,
    IOptions<PluggyOptions> options,
    ILogger<PluggySyncService> logger) : IPluggySyncService
{
    public async Task SyncCreditCardTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var accountId = options.Value.DefaultCreditCardAccountId;
        if (string.IsNullOrWhiteSpace(accountId))
        {
            logger.LogInformation("Skipping Pluggy transaction sync because Pluggy:DefaultCreditCardAccountId is not configured.");
            return;
        }

        var from = DateTime.UtcNow.AddHours(-6);
        var to = DateTime.UtcNow;

        var transactions = await pluggyService.GetCreditCardTransactionsAsync(accountId, from, to, cancellationToken: cancellationToken);
        logger.LogInformation("Pluggy sync completed. Retrieved {Count} credit card transactions for account {AccountId}.", transactions.Count, accountId);
    }
}
