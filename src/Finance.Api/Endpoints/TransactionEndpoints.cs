using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Application.UseCases;

namespace Finance.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions")
            .WithTags("Transactions")
            .RequireAuthorization();

        group.MapGet("/", GetAllTransactions)
            .WithName("GetAllTransactions")
            .WithSummary("Lista todas as transações")
            .Produces<List<Finance.Domain.Entities.Transaction>>()
            .AddEndpointFilter(async (context, next) =>
            {
                // allow parsing of optional query params year and month
                return await next(context);
            });

        group.MapGet("/{id:int}", GetTransactionById)
            .WithName("GetTransactionById")
            .WithSummary("Busca uma transação por ID")
            .Produces<Finance.Domain.Entities.Transaction>()
            .Produces(404);
        
        // Support optional year/month as query parameters via same endpoint (query string)

        group.MapPost("/", CreateTransaction)
            .WithName("CreateTransaction")
            .WithSummary("Cria uma nova transação")
            .Produces(200)
            .Produces(400);

        group.MapPut("/{id:int}", UpdateTransaction)
            .WithName("UpdateTransaction")
            .WithSummary("Atualiza uma transação existente")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:int}", DeleteTransaction)
            .WithName("DeleteTransaction")
            .WithSummary("Exclui uma transação")
            .Produces(200)
            .Produces(400)
            .Produces(404);
    }

    private static async Task<IResult> GetAllTransactions(HttpContext httpContext, ITransactionRepository repository, ICategoryRepository categoryRepo, int? year = null, int? month = null)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        var transactions = await repository.GetByUserIdAsync(userId, year, month);
        // fetch categories for the user to map names
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var catMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var dtos = transactions.Select(t => new Dtos.TransactionResponseDto(
            t.Id,
            t.AccountId,
            t.CategoryId,
            catMap.TryGetValue(t.CategoryId, out var n) ? n : string.Empty,
            t.Amount,
            t.Date,
            t.Description,
            t.Type)).ToList();
        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetTransactionById(int id, ITransactionRepository repository, ICategoryRepository categoryRepo)
    {
        var transaction = await repository.GetByIdAsync(id);
        if (transaction == null) return Results.NotFound();
        var category = await categoryRepo.GetByIdAsync(transaction.CategoryId);
        var dto = new Dtos.TransactionResponseDto(
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            category?.Name ?? string.Empty,
            transaction.Amount,
            transaction.Date,
            transaction.Description,
            transaction.Type);
        return Results.Ok(dto);
    }

    private static async Task<IResult> CreateTransaction(
        HttpContext httpContext,
        CreateTransactionUseCase useCase,
        CreateTransactionRequest request)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            await useCase.ExecuteAsync(
                request.AccountId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description ?? "",
                request.Type,
                userId);

            return Results.Ok(new { message = "Transaction created successfully" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateTransaction(
        HttpContext httpContext,
        int id,
        UpdateTransactionUseCase useCase,
        UpdateTransactionRequest request)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            await useCase.ExecuteAsync(
                id,
                request.AccountId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description ?? "",
                request.Type,
                userId);

            return Results.Ok(new { message = "Transaction updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteTransaction(
        HttpContext httpContext,
        int id,
        DeleteTransactionUseCase useCase)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
                userId = parsed;

            await useCase.ExecuteAsync(id, userId);
            return Results.Ok(new { message = "Transaction deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
