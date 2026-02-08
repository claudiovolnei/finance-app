using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<List<Account>> GetAllAsync();
    Task AddAsync(Account account);
    Task UpdateAsync(Account account, string newName, decimal newInitialBalance);
    Task DeleteAsync(Guid id);
}
