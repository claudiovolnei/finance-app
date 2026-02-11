using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;
using Finance.Infrastructure.Persistence;
using Finance.Application.Repositories;
using System.Reflection;

namespace Finance.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
        => await _context.Accounts.FindAsync(id);

    public Task<List<Account>> GetAllAsync()
        => _context.Accounts.ToListAsync();

    public Task<List<Account>> GetByUserIdAsync(Guid userId)
        => _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

    public async Task AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account, string newName, decimal newInitialBalance)
    {
        typeof(Account).GetProperty("Name", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(account, newName);
        typeof(Account).GetProperty("InitialBalance", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(account, newInitialBalance);
        
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}
