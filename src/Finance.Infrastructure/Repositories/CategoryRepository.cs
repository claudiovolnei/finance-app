using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;
using Finance.Infrastructure.Persistence;
using Finance.Application.Repositories;
using System.Reflection;

namespace Finance.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
        => await _context.Categories.FindAsync(id);

    public Task<List<Category>> GetAllAsync()
        => _context.Categories.ToListAsync();

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category, string newName)
    {
        // Usar reflection para atualizar propriedade privada
        var nameProperty = typeof(Category).GetProperty("Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (nameProperty != null)
        {
            nameProperty.SetValue(category, newName);
        }
        
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
