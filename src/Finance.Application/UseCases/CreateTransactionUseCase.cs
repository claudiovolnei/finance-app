using Finance.Domain.Entities;
using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class CreateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public CreateTransactionUseCase(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task ExecuteAsync(
        int accountId,
        int? categoryId,
        int? transferAccountId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        int userId)
    {
        if (amount <= 0)
            throw new ArgumentException("O valor do lançamento deve ser maior que zero.");

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account is null || account.UserId != userId)
            throw new ArgumentException("Conta principal inválida.");

        if (transferAccountId.HasValue)
        {
            await CreateTransferAsync(accountId, transferAccountId.Value, amount, date, description, type, userId);
            return;
        }

        var transaction = new Transaction(
            accountId,
            categoryId,
            null,
            amount,
            date,
            description,
            type,
            userId);

        await _transactionRepository.AddAsync(transaction);
    }

    private async Task CreateTransferAsync(int accountId, int transferAccountId, decimal amount, DateTime date, string description, TransactionType type, int userId)
    {
        if (accountId == transferAccountId)
            throw new ArgumentException("A conta de transferência deve ser diferente da conta atual.");

        var destinationAccount = await _accountRepository.GetByIdAsync(transferAccountId);
        if (destinationAccount is null || destinationAccount.UserId != userId)
            throw new ArgumentException("Conta de transferência inválida.");

        var fromAccountId = type == TransactionType.Expense ? accountId : transferAccountId;
        var availableBalance = await _transactionRepository.GetAccountBalanceAsync(userId, fromAccountId);

        if (availableBalance < amount)
            throw new InvalidOperationException("Saldo insuficiente para realizar a transferência.");

        var debitTransaction = new Transaction(
            fromAccountId,
            null,
            fromAccountId == accountId ? transferAccountId : accountId,
            amount,
            date,
            description,
            TransactionType.Expense,
            userId);

        var creditTransaction = new Transaction(
            fromAccountId == accountId ? transferAccountId : accountId,
            null,
            fromAccountId,
            amount,
            date,
            description,
            TransactionType.Income,
            userId);

        await _transactionRepository.AddRangeAsync([debitTransaction, creditTransaction]);
    }
}
