using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<List<Account>> GetAllAsync();
    Task AddAsync(Account account);
    Task UpdateAsync(Account account, string newName, decimal newInitialBalance);
    Task<List<Account>> GetByUserIdAsync(int userId);
    Task DeleteAsync(int id);
}
