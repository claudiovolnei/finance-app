namespace Finance.Application.Services;

public interface IPluggySyncService
{
    Task SyncCreditCardTransactionsAsync(CancellationToken cancellationToken = default);
}
