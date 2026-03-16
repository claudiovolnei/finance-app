using Finance.Application.Models.OpenFinance;

namespace Finance.Application.Services;

public interface IPluggyService
{
    Task<ConnectBankAccountResultModel> ConnectBankAccountAsync(
        ConnectBankAccountRequestModel request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CreditCardTransactionModel>> GetCreditCardTransactionsAsync(
        string accountId,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
