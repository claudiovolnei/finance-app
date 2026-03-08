using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;
using Finance.Infrastructure.Persistence;
using Finance.Application.Repositories;

namespace Finance.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories.FindAsync(id);

    public Task<List<Category>> GetAllAsync()
        => _context.Categories.ToListAsync();

    public Task<List<Category>> GetByUserIdAsync(int userId)
        => _context.Categories.Where(c => c.UserId == userId).ToListAsync();

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category, string newName)
    {
        category.UpdateName(newName);

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
