using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<List<Transaction>> GetAllAsync();
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction, int accountId, int categoryId, decimal amount, DateTime date, string description, TransactionType type); // Trigger rebuild
    Task DeleteAsync(int id);
    Task<List<Transaction>> GetByUserIdAsync(int userId, int? year = null, int? month = null);
}