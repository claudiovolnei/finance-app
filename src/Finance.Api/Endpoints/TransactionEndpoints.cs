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
            .Produces<List<Finance.Domain.Entities.Transaction>>();

        group.MapGet("/{id:guid}", GetTransactionById)
            .WithName("GetTransactionById")
            .WithSummary("Busca uma transação por ID")
            .Produces<Finance.Domain.Entities.Transaction>()
            .Produces(404);

        group.MapPost("/", CreateTransaction)
            .WithName("CreateTransaction")
            .WithSummary("Cria uma nova transação")
            .Produces(200)
            .Produces(400);

        group.MapPut("/{id:guid}", UpdateTransaction)
            .WithName("UpdateTransaction")
            .WithSummary("Atualiza uma transação existente")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:guid}", DeleteTransaction)
            .WithName("DeleteTransaction")
            .WithSummary("Exclui uma transação")
            .Produces(200)
            .Produces(400)
            .Produces(404);
    }

    private static async Task<IResult> GetAllTransactions(HttpContext httpContext, ITransactionRepository repository)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        Guid userId = Guid.Empty;
        if (userClaim != null && Guid.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        var transactions = await repository.GetByUserIdAsync(userId);
        return Results.Ok(transactions);
    }

    private static async Task<IResult> GetTransactionById(Guid id, ITransactionRepository repository)
    {
        var transaction = await repository.GetByIdAsync(id);
        if (transaction == null) return Results.NotFound();
        // ensure same user
        return Results.Ok(transaction);
    }

    private static async Task<IResult> CreateTransaction(
        HttpContext httpContext,
        CreateTransactionUseCase useCase,
        CreateTransactionRequest request)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid userId = Guid.Empty;
            if (userClaim != null && Guid.TryParse(userClaim.Value, out var parsed))
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
        Guid id,
        UpdateTransactionUseCase useCase,
        UpdateTransactionRequest request)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid userId = Guid.Empty;
            if (userClaim != null && Guid.TryParse(userClaim.Value, out var parsed))
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
        Guid id,
        DeleteTransactionUseCase useCase)
    {
        try
        {
            var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid userId = Guid.Empty;
            if (userClaim != null && Guid.TryParse(userClaim.Value, out var parsed))
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
