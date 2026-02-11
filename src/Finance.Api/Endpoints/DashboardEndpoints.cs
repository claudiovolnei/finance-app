using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;

namespace Finance.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard").WithTags("Dashboard");

        group.MapGet("/summary", GetSummary)
            .WithName("GetDashboardSummary")
            .WithSummary("Retorna resumo do dashboard (saldo, receitas, despesas, categorias, últimos lançamentos)")
            .Produces<DashboardSummaryDto>();
    }

    private static async Task<IResult> GetSummary(ITransactionRepository transactionRepo, ICategoryRepository categoryRepo)
    {
        var transactions = await transactionRepo.GetAllAsync();
        var categories = await categoryRepo.GetAllAsync();

        var totalIncome = transactions.Where(t => t.Type == Finance.Domain.Entities.TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == Finance.Domain.Entities.TransactionType.Expense).Sum(t => t.Amount);
        var balance = totalIncome - totalExpense;

        var categoryTotals = transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(t => t.Amount) })
            .ToList();

        var totalByCategories = categoryTotals.Sum(c => Math.Abs(c.Amount));

        var categorySummaries = categoryTotals.Select(c => {
            var cat = categories.FirstOrDefault(x => x.Id == c.CategoryId);
            var name = cat?.Name ?? "Outros";
            var percent = totalByCategories == 0 ? 0m : Math.Round((decimal)(Math.Abs(c.Amount) / totalByCategories) * 100m, 2);
            return new CategorySummaryDto(c.CategoryId, name, c.Amount, percent);
        }).ToList();

        var latest = transactions.OrderByDescending(t => t.Date).Take(5)
            .Select(t => {
                var cat = categories.FirstOrDefault(c => c.Id == t.CategoryId);
                return new TransactionSummaryDto(t.Id, t.Date, t.Description, t.CategoryId, cat?.Name ?? string.Empty, t.Amount, t.Type);
            }).ToList();

        var dto = new DashboardSummaryDto(balance, totalIncome, totalExpense, categorySummaries, latest);

        return Results.Ok(dto);
    }
}
