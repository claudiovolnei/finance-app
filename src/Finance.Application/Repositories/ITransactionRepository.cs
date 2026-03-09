using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<List<Transaction>> GetAllAsync();
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateAsync(Transaction transaction, int accountId, int? categoryId, int? transferAccountId, decimal amount, DateTime date, string description, TransactionType type);
    Task DeleteAsync(int id);
    Task<List<Transaction>> GetByUserIdAsync(int userId, int? year = null, int? month = null, int? accountId = null);
    Task<decimal> GetBalanceTotal(int userId, int year, int accountId);
    Task<decimal> GetAccountBalanceAsync(int userId, int accountId);
    Task<Transaction?> FindTransferCounterpartAsync(Transaction transaction);
}
