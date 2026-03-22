using Finance.Domain.Entities;
using Finance.Application.Repositories;

namespace Finance.Application.UseCases;

public class UpdateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public UpdateTransactionUseCase(ITransactionRepository transactionRepository, IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task ExecuteAsync(
        int id,
        int accountId,
        int? categoryId,
        int? transferAccountId,
        decimal amount,
        DateTime date,
        string description,
        TransactionType type,
        int userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            throw new KeyNotFoundException($"Transaction with id {id} not found");

        if (transaction.UserId != userId)
            throw new UnauthorizedAccessException("Not authorized");

        if (amount <= 0)
            throw new ArgumentException("O valor do lançamento deve ser maior que zero.");

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account is null || account.UserId != userId)
            throw new ArgumentException("Conta principal inválida.");

        if (transferAccountId.HasValue)
        {
            await UpdateTransferAsync(transaction, account, transferAccountId.Value, amount, date, description, type, userId);
            return;
        }

        if (transaction.TransferAccountId.HasValue)
        {
            var counterpart = await _transactionRepository.FindTransferCounterpartAsync(transaction);
            if (counterpart is not null)
                await _transactionRepository.DeleteAsync(counterpart.Id);
        }

        await _transactionRepository.UpdateAsync(transaction, accountId, categoryId, null, amount, date, description, type);
    }

    private async Task UpdateTransferAsync(Transaction transaction, Account account, int transferAccountId, decimal amount, DateTime date, string description, TransactionType type, int userId)
    {
        if (account.Type != AccountType.Checking)
            throw new ArgumentException("Transferências só podem ser iniciadas a partir de contas correntes.");

        if (account.Id == transferAccountId)
            throw new ArgumentException("A conta de transferência deve ser diferente da conta atual.");

        var destinationAccount = await _accountRepository.GetByIdAsync(transferAccountId);
        if (destinationAccount is null || destinationAccount.UserId != userId)
            throw new ArgumentException("Conta de transferência inválida.");

        if (destinationAccount.Type != AccountType.Checking)
            throw new ArgumentException("Transferências só podem ocorrer entre contas correntes.");

        var fromAccountId = type == TransactionType.Expense ? account.Id : transferAccountId;
        var availableBalance = await _transactionRepository.GetAccountBalanceAsync(userId, fromAccountId);

        var existingDebitFromAccount = transaction.Type == TransactionType.Expense ? transaction.AccountId : transaction.TransferAccountId;
        if (existingDebitFromAccount == fromAccountId)
            availableBalance += transaction.Amount;

        if (availableBalance < amount)
            throw new InvalidOperationException("Saldo insuficiente para realizar a transferência.");

        var counterpart = await _transactionRepository.FindTransferCounterpartAsync(transaction);

        var debitAccountId = fromAccountId;
        var creditAccountId = fromAccountId == account.Id ? transferAccountId : account.Id;

        if (transaction.Type == TransactionType.Expense)
        {
            await _transactionRepository.UpdateAsync(transaction, debitAccountId, null, creditAccountId, amount, date, description, TransactionType.Expense);

            if (counterpart is not null)
                await _transactionRepository.UpdateAsync(counterpart, creditAccountId, null, debitAccountId, amount, date, description, TransactionType.Income);
            else
                await _transactionRepository.AddAsync(new Transaction(creditAccountId, null, debitAccountId, amount, date, description, TransactionType.Income, userId));
        }
        else
        {
            await _transactionRepository.UpdateAsync(transaction, creditAccountId, null, debitAccountId, amount, date, description, TransactionType.Income);

            if (counterpart is not null)
                await _transactionRepository.UpdateAsync(counterpart, debitAccountId, null, creditAccountId, amount, date, description, TransactionType.Expense);
            else
                await _transactionRepository.AddAsync(new Transaction(debitAccountId, null, creditAccountId, amount, date, description, TransactionType.Expense, userId));
        }
    }
}
