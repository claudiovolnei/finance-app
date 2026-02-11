using Finance.Domain.Entities;

namespace Finance.Application.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<List<Category>> GetAllAsync();
    Task AddAsync(Category category);
    Task UpdateAsync(Category category, string newName);
    Task<List<Category>> GetByUserIdAsync(Guid userId);
    Task DeleteAsync(Guid id);
}
