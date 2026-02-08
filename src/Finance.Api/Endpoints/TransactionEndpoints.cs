using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Application.UseCases;

namespace Finance.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions")
            .WithTags("Transactions");

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

    private static async Task<IResult> GetAllTransactions(ITransactionRepository repository)
    {
        var transactions = await repository.GetAllAsync();
        return Results.Ok(transactions);
    }

    private static async Task<IResult> GetTransactionById(Guid id, ITransactionRepository repository)
    {
        var transaction = await repository.GetByIdAsync(id);
        return transaction != null ? Results.Ok(transaction) : Results.NotFound();
    }

    private static async Task<IResult> CreateTransaction(
        CreateTransactionUseCase useCase,
        CreateTransactionRequest request)
    {
        try
        {
            await useCase.ExecuteAsync(
                request.AccountId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description ?? "",
                request.Type);

            return Results.Ok(new { message = "Transaction created successfully" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateTransaction(
        Guid id,
        UpdateTransactionUseCase useCase,
        UpdateTransactionRequest request)
    {
        try
        {
            await useCase.ExecuteAsync(
                id,
                request.AccountId,
                request.CategoryId,
                request.Amount,
                request.Date,
                request.Description ?? "",
                request.Type);

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
        Guid id,
        DeleteTransactionUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id);
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
