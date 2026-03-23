using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Domain.Entities;
using System.Security.Claims;

namespace Finance.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard").WithTags("Dashboard").RequireAuthorization();

        group.MapGet("/summary", GetSummary)
            .WithName("GetDashboardSummary")
            .WithSummary("Retorna resumo do dashboard (saldo, receitas, despesas, categorias, últimos lançamentos)")
            .Produces<DashboardSummaryDto>();

        group.MapGet("/category-expenses/{categoryId:int}", GetCategoryExpenses)
            .WithName("GetCategoryExpenses")
            .WithSummary("Retorna os gastos de uma categoria no mês")
            .Produces<CategoryExpenseDetailDto>();

        group.MapGet("/category-transactions", GetCategoryTransactions)
            .WithName("GetCategoryTransactions")
            .WithSummary("Retorna lançamentos por categoria/tipo, incluindo transferências")
            .Produces<CategoryTransactionsDetailDto>();
    }

    private static async Task<IResult> GetSummary(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, IAccountRepository accountRepository, int? year, int? month, int? accountId)
    {
        var userId = GetUserId(httpContext);
        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month, accountId);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var accounts = await accountRepository.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var creditCardAccounts = accounts.Where(a => a.Type == AccountType.CreditCard).ToList();
        var totalCreditCardAccounts = accountId.HasValue
            ? creditCardAccounts.Count(a => a.ParentAccountId == accountId.Value)
            : creditCardAccounts.Count;
        var totalCreditCardDebt = transactions
            .Where(t => t.Type == TransactionType.Expense && accountMap.TryGetValue(t.AccountId, out var account) && account.Type == AccountType.CreditCard)
            .Sum(t => t.Amount);

        var currentAccounts = accounts.Where(a => a.Type == AccountType.Checking).ToList();
        var scopedCurrentAccounts = accountId.HasValue
            ? currentAccounts.Where(a => a.Id == accountId.Value).ToList()
            : currentAccounts;

        decimal balance = 0m;
        foreach (var currentAccount in scopedCurrentAccounts)
        {
            balance += await transactionRepo.GetAccountBalanceAsync(userId, currentAccount.Id);
        }

        var categoryTotals = transactions
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new { g.Key.CategoryId, g.Key.Type, Amount = g.Sum(t => t.Amount) })
            .ToList();

        var totalsByType = categoryTotals
            .GroupBy(x => x.Type)
            .ToDictionary(g => g.Key, g => g.Sum(x => Math.Abs(x.Amount)));

        var categorySummaries = new List<CategorySummaryDto>();
        foreach (var c in categoryTotals)
        {
            var name = c.CategoryId.HasValue && categoryMap.TryGetValue(c.CategoryId.Value, out var categoryName)
                ? categoryName
                : "Transferência";
            var typeTotal = totalsByType.TryGetValue(c.Type, out var total) ? total : 0m;
            var percent = typeTotal == 0 ? 0m : Math.Round((Math.Abs(c.Amount) / typeTotal) * 100m, 2);
            categorySummaries.Add(new CategorySummaryDto(c.CategoryId, name, c.Amount, percent, c.Type));
        }

        var creditCardExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && accountMap.TryGetValue(t.AccountId, out var expenseAccount) && expenseAccount.Type == AccountType.CreditCard)
            .GroupBy(t => t.AccountId)
            .Select(g => new CreditCardExpenseSummaryDto(g.Key, accountMap[g.Key].Name, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Amount)
            .ThenBy(x => x.AccountName)
            .ToList();

        var latestTxs = transactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(12)
            .ToList();

        var latest = BuildTransactionSummaries(latestTxs, categoryMap, accountMap);

        var dto = new DashboardSummaryDto(balance, totalIncome, totalExpense, totalCreditCardDebt, totalCreditCardAccounts, categorySummaries, creditCardExpenses, latest);
        return Results.Ok(dto);
    }

    private static async Task<IResult> GetCategoryTransactions(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, IAccountRepository accountRepository, TransactionType type, int? categoryId, int? year, int? month, int? accountId)
    {
        var userId = GetUserId(httpContext);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var accounts = await accountRepository.GetByUserIdAsync(userId);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month, accountId);

        var filtered = transactions
            .Where(t => t.Type == type && t.CategoryId == categoryId)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Id)
            .ToList();

        var title = categoryId.HasValue && categoryMap.TryGetValue(categoryId.Value, out var categoryName)
            ? categoryName
            : "Transferência";

        var summaries = BuildTransactionSummaries(filtered, categoryMap, accountMap);
        return Results.Ok(new CategoryTransactionsDetailDto(categoryId, title, filtered.Sum(t => t.Amount), type, summaries));
    }

    private static async Task<IResult> GetCategoryExpenses(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, IAccountRepository accountRepository, int categoryId, int? year, int? month, int? accountId)
    {
        var userId = GetUserId(httpContext);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var category = categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
            return Results.NotFound();

        var accounts = await accountRepository.GetByUserIdAsync(userId);
        var accountMap = accounts.ToDictionary(a => a.Id);
        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month, accountId);
        var filtered = transactions
            .Where(t => t.Type == TransactionType.Expense && t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToList();

        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var summaries = BuildTransactionSummaries(filtered, categoryMap, accountMap);
        return Results.Ok(new CategoryExpenseDetailDto(categoryId, category.Name, filtered.Sum(t => t.Amount), summaries));
    }

    private static List<TransactionSummaryDto> BuildTransactionSummaries(List<Transaction> transactions, Dictionary<int, string> categoryMap, IReadOnlyDictionary<int, Account> accountMap)
    {
        return transactions.Select(t =>
        {
            accountMap.TryGetValue(t.AccountId, out var account);
            Account? transferAccount = null;
            if (t.TransferAccountId.HasValue)
                accountMap.TryGetValue(t.TransferAccountId.Value, out transferAccount);

            Account? parentAccount = null;
            if (account?.ParentAccountId is int parentAccountId)
                accountMap.TryGetValue(parentAccountId, out parentAccount);

            var categoryName = t.CategoryId.HasValue && categoryMap.TryGetValue(t.CategoryId.Value, out var category)
                ? category
                : "Transferência";

            return new TransactionSummaryDto(
                t.Id,
                t.AccountId,
                account?.Name ?? "Conta desconhecida",
                account?.Type ?? AccountType.Checking,
                account?.ParentAccountId,
                parentAccount?.Name,
                account?.Type == AccountType.CreditCard,
                t.CategoryId,
                categoryName,
                t.Amount,
                t.Date,
                t.Description,
                t.Type,
                t.TransferAccountId,
                transferAccount?.Name ?? "N/A");
        }).ToList();
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            return parsed;

        return 0;
    }
}
