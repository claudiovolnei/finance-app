using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;
using Finance.Infrastructure.Persistence;
using Finance.Application.Repositories;

namespace Finance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
        => await _context.Transactions.FindAsync(id);

    public Task<List<Transaction>> GetAllAsync()
        => _context.Transactions.ToListAsync();

    public Task<List<Transaction>> GetByUserIdAsync(int userId, int? year = null, int? month = null)
    {
        var q = _context.Transactions.Where(t => t.UserId == userId).AsQueryable();
        if (year.HasValue)
            q = q.Where(t => t.Date.Year == year.Value);
        if (month.HasValue)
            q = q.Where(t => t.Date.Month == month.Value);
        return q.ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction, int accountId, int categoryId, decimal amount, DateTime date, string description, TransactionType type)
    {
        transaction.Update(accountId, categoryId, amount, date, description, type);

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}