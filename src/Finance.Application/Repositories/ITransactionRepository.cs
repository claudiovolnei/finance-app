using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<List<Transaction>> GetAllAsync();
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction, Guid accountId, Guid categoryId, decimal amount, DateTime date, string description, TransactionType type);
    Task DeleteAsync(Guid id);
}