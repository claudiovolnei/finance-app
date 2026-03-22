using Finance.Application.Repositories;
using Finance.Domain.Entities;

namespace Finance.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts")
            .WithTags("Accounts")
            .RequireAuthorization();

        group.MapGet("/", GetAllAccounts)
            .WithName("GetAllAccounts")
            .WithSummary("Lista todas as contas")
            .Produces<List<Account>>();

        group.MapGet("/{id:int}", GetAccountById)
            .WithName("GetAccountById")
            .WithSummary("Busca uma conta por ID")
            .Produces<Account>()
            .Produces(404);

        group.MapPost("/", CreateAccount)
            .WithName("CreateAccount")
            .WithSummary("Cria uma nova conta")
            .Produces<Account>(201)
            .Produces(400);

        group.MapPut("/{id:int}", UpdateAccount)
            .WithName("UpdateAccount")
            .WithSummary("Atualiza uma conta")
            .Produces(204)
            .Produces(404)
            .Produces(400);

        group.MapDelete("/{id:int}", DeleteAccount)
            .WithName("DeleteAccount")
            .WithSummary("Remove uma conta")
            .Produces(204)
            .Produces(404);
    }

    private static int GetUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userClaim != null && int.TryParse(userClaim.Value, out var parsed) ? parsed : 0;
    }

    private static async Task<IResult> GetAllAccounts(HttpContext httpContext, IAccountRepository repository)
    {
        var userId = GetUserId(httpContext);
        var accounts = await repository.GetByUserIdAsync(userId);
        return Results.Ok(accounts);
    }

    private static async Task<IResult> GetAccountById(HttpContext httpContext, int id, IAccountRepository repository)
    {
        var account = await repository.GetByIdAsync(id);
        if (account == null)
            return Results.NotFound();

        var userId = GetUserId(httpContext);
        if (account.UserId != userId)
            return Results.Forbid();

        return Results.Ok(account);
    }

    private static async Task<IResult> CreateAccount(HttpContext httpContext, CreateAccountRequest request, IAccountRepository repository)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Nome da conta é obrigatório.");

        var userId = GetUserId(httpContext);
        var validationResult = await ValidateAccountRequestAsync(request.Type, request.ParentAccountId, userId, repository);
        if (validationResult is not null)
            return validationResult;

        var initialBalance = request.Type == AccountType.CreditCard ? 0m : request.InitialBalance;
        var account = new Account(request.Name.Trim(), initialBalance, userId, request.Type, request.ParentAccountId);
        await repository.AddAsync(account);

        return Results.Created($"/accounts/{account.Id}", account);
    }

    private static async Task<IResult> UpdateAccount(HttpContext httpContext, int id, UpdateAccountRequest request, IAccountRepository repository)
    {
        var account = await repository.GetByIdAsync(id);
        if (account == null)
            return Results.NotFound();

        var userId = GetUserId(httpContext);
        if (account.UserId != userId)
            return Results.Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Nome da conta é obrigatório.");

        var validationResult = await ValidateAccountRequestAsync(request.Type, request.ParentAccountId, userId, repository, id);
        if (validationResult is not null)
            return validationResult;

        var initialBalance = request.Type == AccountType.CreditCard ? 0m : request.InitialBalance;
        await repository.UpdateAsync(account, request.Name.Trim(), initialBalance, request.Type, request.ParentAccountId);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAccount(HttpContext httpContext, int id, IAccountRepository repository)
    {
        var account = await repository.GetByIdAsync(id);
        if (account == null)
            return Results.NotFound();

        var userId = GetUserId(httpContext);
        if (account.UserId != userId)
            return Results.Forbid();

        await repository.DeleteAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult?> ValidateAccountRequestAsync(AccountType type, int? parentAccountId, int userId, IAccountRepository repository, int? accountId = null)
    {
        if (type == AccountType.CreditCard)
        {
            if (!parentAccountId.HasValue)
                return Results.BadRequest("Cartão de crédito precisa estar vinculado a uma conta corrente.");

            if (accountId.HasValue && parentAccountId.Value == accountId.Value)
                return Results.BadRequest("O cartão de crédito não pode ser vinculado a ele mesmo.");

            var parentAccount = await repository.GetByIdAsync(parentAccountId.Value);
            if (parentAccount is null || parentAccount.UserId != userId)
                return Results.BadRequest("Conta vinculada inválida.");

            if (parentAccount.Type != AccountType.Checking)
                return Results.BadRequest("O cartão de crédito só pode ser vinculado a uma conta corrente.");
        }
        else if (parentAccountId.HasValue)
        {
            return Results.BadRequest("Conta corrente não pode possuir conta vinculada.");
        }

        return null;
    }

    private sealed record CreateAccountRequest(string Name, decimal InitialBalance, AccountType Type, int? ParentAccountId);
    private sealed record UpdateAccountRequest(string Name, decimal InitialBalance, AccountType Type, int? ParentAccountId);
}
