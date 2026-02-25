using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
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
    }

    private static async Task<IResult> GetSummary(HttpContext httpContext, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo, int? year, int? month)
    {
        // get user id from claims
        var userClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        var transactions = await transactionRepo.GetByUserIdAsync(userId, year, month);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var totalIncome = transactions.Where(t => t.Type == Finance.Domain.Entities.TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == Finance.Domain.Entities.TransactionType.Expense).Sum(t => t.Amount);
        var balance = totalIncome - totalExpense;

        var categoryTotals = transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToList();

        var totalByCategories = categoryTotals.Sum(c => Math.Abs(c.Amount));

        var categorySummaries = new List<CategorySummaryDto>();
        foreach (var c in categoryTotals)
        {
            var name = categoryMap.TryGetValue(c.CategoryId, out var categoryName) ? categoryName : "Outros";
            var percent = totalByCategories == 0 ? 0m : Math.Round((decimal)(Math.Abs(c.Amount) / totalByCategories) * 100m, 2);
            categorySummaries.Add(new CategorySummaryDto(c.CategoryId, name, c.Amount, percent));
        }

        var latestTxs = transactions.OrderByDescending(t => t.Date).Take(5).ToList();
        var latest = new List<TransactionSummaryDto>();
        foreach (var t in latestTxs)
        {
            var categoryName = categoryMap.TryGetValue(t.CategoryId, out var category) ? category : string.Empty;
            latest.Add(new TransactionSummaryDto(t.Id, t.Date, t.Description, t.CategoryId, categoryName, t.Amount, t.Type));
        }

        var dto = new DashboardSummaryDto(balance, totalIncome, totalExpense, categorySummaries, latest);

        return Results.Ok(dto);
    }
}
