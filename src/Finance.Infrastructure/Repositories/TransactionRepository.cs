using Microsoft.EntityFrameworkCore;
using Finance.Domain.Entities;
using Finance.Infrastructure.Persistence;
using Finance.Application.Repositories;
using System.Reflection;

namespace Finance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _context.Transactions.FindAsync(id);

    public Task<List<Transaction>> GetAllAsync()
        => _context.Transactions.ToListAsync();

    public Task<List<Transaction>> GetByUserIdAsync(Guid userId)
        => _context.Transactions.Where(t => t.UserId == userId).ToListAsync();

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction, Guid accountId, Guid categoryId, decimal amount, DateTime date, string description, TransactionType type)
    {
        // Usar reflection para atualizar propriedades privadas
        typeof(Transaction).GetProperty("AccountId", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, accountId);
        typeof(Transaction).GetProperty("CategoryId", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, categoryId);
        typeof(Transaction).GetProperty("Amount", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, amount);
        typeof(Transaction).GetProperty("Date", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, date);
        typeof(Transaction).GetProperty("Description", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, description);
        typeof(Transaction).GetProperty("Type", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(transaction, type);

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}