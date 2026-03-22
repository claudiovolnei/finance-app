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

    public async Task<List<Transaction>> GetByUserIdAsync(int userId, int? year = null, int? month = null, int? accountId = null)
    {
        var query = _context.Transactions.Where(t => t.UserId == userId).AsQueryable();
        if (year.HasValue)
            query = query.Where(t => t.Date.Year == year.Value);
        if (month.HasValue)
            query = query.Where(t => t.Date.Month == month.Value);
        if (accountId.HasValue)
        {
            var scopeAccountIds = await _context.Accounts
                .Where(a => a.UserId == userId && (a.Id == accountId.Value || a.ParentAccountId == accountId.Value))
                .Select(a => a.Id)
                .ToListAsync();

            query = query.Where(t => scopeAccountIds.Contains(t.AccountId));
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction, int accountId, int? categoryId, int? transferAccountId, decimal amount, DateTime date, string description, TransactionType type)
    {
        transaction.Update(accountId, categoryId, transferAccountId, amount, date, description, type);

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

    public async Task<decimal> GetBalanceTotal(int userId, int year, int accountId)
    {
        var accountIds = accountId > 0
            ? await _context.Accounts.Where(a => a.UserId == userId && (a.Id == accountId || a.ParentAccountId == accountId)).Select(a => a.Id).ToListAsync()
            : await _context.Accounts.Where(a => a.UserId == userId && a.Type == AccountType.Checking).Select(a => a.Id).ToListAsync();

        if (accountIds.Count == 0)
            return 0m;

        return await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Year == year && accountIds.Contains(t.AccountId))
            .SumAsync(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
    }

    public async Task<decimal> GetAccountBalanceAsync(int userId, int accountId)
    {
        var initialBalance = await _context.Accounts
            .Where(a => a.UserId == userId && a.Id == accountId)
            .Select(a => a.InitialBalance)
            .FirstOrDefaultAsync();

        var transactionsBalance = await _context.Transactions
            .Where(t => t.UserId == userId && t.AccountId == accountId)
            .SumAsync(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);

        return initialBalance + transactionsBalance;
    }

    public Task<Transaction?> FindTransferCounterpartAsync(Transaction transaction)
    {
        var oppositeType = transaction.Type == TransactionType.Income ? TransactionType.Expense : TransactionType.Income;

        return _context.Transactions
            .Where(t => t.Id != transaction.Id
                        && t.UserId == transaction.UserId
                        && t.AccountId == transaction.TransferAccountId
                        && t.TransferAccountId == transaction.AccountId
                        && t.Amount == transaction.Amount
                        && t.Date == transaction.Date
                        && t.Type == oppositeType
                        && t.Description == transaction.Description)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();
    }
}
