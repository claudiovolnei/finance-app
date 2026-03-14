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
    }

    private static async Task<IResult> GetSummary(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, IAccountRepository accountRepository, int? year, int? month, int? accountId)
    {
        var userId = GetUserId(httpContext);

        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month, accountId);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var balance = await transactionRepo.GetBalanceTotal(userId, year ?? DateTime.Now.Year, accountId ?? 0);

        var categoryTotals = transactions
            .Where(t => t.CategoryId.HasValue)
            .GroupBy(t => new { CategoryId = t.CategoryId!.Value, t.Type })
            .Select(g => new { g.Key.CategoryId, g.Key.Type, Amount = g.Sum(t => t.Amount) })
            .ToList();

        var totalsByType = categoryTotals
            .GroupBy(x => x.Type)
            .ToDictionary(g => g.Key, g => g.Sum(x => Math.Abs(x.Amount)));

        var categorySummaries = new List<CategorySummaryDto>();
        foreach (var c in categoryTotals)
        {
            var name = categoryMap.TryGetValue(c.CategoryId, out var categoryName) ? categoryName : "Outros";
            var typeTotal = totalsByType.TryGetValue(c.Type, out var total) ? total : 0m;
            var percent = typeTotal == 0 ? 0m : Math.Round((Math.Abs(c.Amount) / typeTotal) * 100m, 2);
            categorySummaries.Add(new CategorySummaryDto(c.CategoryId, name, c.Amount, percent, c.Type));
        }

        var latestTxs = transactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(12)
            .ToList();

        var latest = await BuildTransactionSummariesAsync(latestTxs, categoryMap, accountRepository);

        var dto = new DashboardSummaryDto(balance, totalIncome, totalExpense, categorySummaries, latest);

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetCategoryExpenses(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, IAccountRepository accountRepository, int categoryId, int? year, int? month, int? accountId)
    {
        var userId = GetUserId(httpContext);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var category = categories.FirstOrDefault(c => c.Id == categoryId);
        if (category is null)
            return Results.NotFound();

        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month, accountId);
        var filtered = transactions
            .Where(t => t.Type == TransactionType.Expense && t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToList();

        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var summaries = await BuildTransactionSummariesAsync(filtered, categoryMap, accountRepository);

        return Results.Ok(new CategoryExpenseDetailDto(categoryId, category.Name, filtered.Sum(t => t.Amount), summaries));
    }

    private static async Task<List<TransactionSummaryDto>> BuildTransactionSummariesAsync(
        List<Transaction> transactions,
        Dictionary<int, string> categoryMap,
        IAccountRepository accountRepository)
    {
        var latest = new List<TransactionSummaryDto>();

        foreach (var t in transactions)
        {
            var categoryName = t.CategoryId.HasValue && categoryMap.TryGetValue(t.CategoryId.Value, out var category) ? category : "Transferência";
            var accountName = "N/A";
            if (t.TransferAccountId.HasValue)
            {
                var account = await accountRepository.GetByIdAsync(t.TransferAccountId.Value);
                accountName = account?.Name ?? "N/A";
            }

            latest.Add(new TransactionSummaryDto(t.Id, t.Date, t.Description, t.CategoryId, categoryName, t.Amount, t.Type, accountName));
        }

        return latest;
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            return parsed;

        return 0;
    }
}
