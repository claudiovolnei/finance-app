using Finance.Api.Endpoints.Dtos;
using Finance.Application.Repositories;
using Finance.Application.UseCases;
using Finance.Domain.Entities;

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
            .Produces<List<TransactionResponseDto>>();

        group.MapGet("/{id:int}", GetTransactionById)
            .WithName("GetTransactionById")
            .WithSummary("Busca uma transação por ID")
            .Produces<TransactionResponseDto>()
            .Produces(404);

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

    private static async Task<IResult> GetAllTransactions(HttpContext httpContext, ITransactionRepository repository, ICategoryRepository categoryRepo, IAccountRepository accountRepository, int? year = null, int? month = null, int? accountId = null)
    {
        var userId = GetUserId(httpContext);
        var transactions = await repository.GetByUserIdAsync(userId, year, month, accountId);
        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var accounts = await accountRepository.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var accountMap = accounts.ToDictionary(a => a.Id);

        var dtos = transactions
            .Select(t => MapTransactionDto(t, categoryMap, accountMap))
            .OrderByDescending(o => o.Date)
            .ThenByDescending(o => o.Id)
            .ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetTransactionById(HttpContext httpContext, int id, ITransactionRepository repository, ICategoryRepository categoryRepo, IAccountRepository accountRepository)
    {
        var transaction = await repository.GetByIdAsync(id);
        if (transaction == null)
            return Results.NotFound();

        var userId = GetUserId(httpContext);
        if (transaction.UserId != userId)
            return Results.Forbid();

        var categories = await categoryRepo.GetByUserIdAsync(userId);
        var accounts = await accountRepository.GetByUserIdAsync(userId);
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var accountMap = accounts.ToDictionary(a => a.Id);

        return Results.Ok(MapTransactionDto(transaction, categoryMap, accountMap));
    }

    private static async Task<IResult> CreateTransaction(HttpContext httpContext, CreateTransactionUseCase useCase, CreateTransactionRequest request)
    {
        try
        {
            await useCase.ExecuteAsync(
                request.AccountId,
                request.CategoryId,
                request.TransferAccountId,
                request.Amount,
                request.Date,
                request.Description ?? string.Empty,
                request.Type,
                GetUserId(httpContext));

            return Results.Ok(new { message = "Transaction created successfully" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateTransaction(HttpContext httpContext, int id, UpdateTransactionUseCase useCase, UpdateTransactionRequest request)
    {
        try
        {
            await useCase.ExecuteAsync(
                id,
                request.AccountId,
                request.CategoryId,
                request.TransferAccountId,
                request.Amount,
                request.Date,
                request.Description ?? string.Empty,
                request.Type,
                GetUserId(httpContext));

            return Results.Ok(new { message = "Transaction updated successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteTransaction(HttpContext httpContext, int id, DeleteTransactionUseCase useCase)
    {
        try
        {
            await useCase.ExecuteAsync(id, GetUserId(httpContext));
            return Results.Ok(new { message = "Transaction deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static TransactionResponseDto MapTransactionDto(Transaction transaction, IReadOnlyDictionary<int, string> categoryMap, IReadOnlyDictionary<int, Account> accountMap)
    {
        accountMap.TryGetValue(transaction.AccountId, out var account);
        Account? transferAccount = null;
        if (transaction.TransferAccountId.HasValue)
            accountMap.TryGetValue(transaction.TransferAccountId.Value, out transferAccount);

        Account? parentAccount = null;
        if (account?.ParentAccountId is int parentAccountId)
            accountMap.TryGetValue(parentAccountId, out parentAccount);

        return new TransactionResponseDto(
            transaction.Id,
            transaction.AccountId,
            account?.Name ?? "Conta desconhecida",
            account?.Type ?? AccountType.Checking,
            account?.ParentAccountId,
            parentAccount?.Name,
            account?.Type == AccountType.CreditCard,
            transaction.CategoryId,
            transaction.TransferAccountId,
            transferAccount?.Name ?? "N/A",
            transaction.CategoryId.HasValue && categoryMap.TryGetValue(transaction.CategoryId.Value, out var categoryName)
                ? categoryName
                : "Transferência",
            transaction.Amount,
            transaction.Date,
            transaction.Description,
            transaction.Type);
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userClaim != null && int.TryParse(userClaim.Value, out var parsed) ? parsed : 0;
    }
}
