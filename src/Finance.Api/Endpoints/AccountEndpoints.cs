using Finance.Application.Repositories;

namespace Finance.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts")
            .WithTags("Accounts");

        group.MapGet("/", GetAllAccounts)
            .WithName("GetAllAccounts")
            .WithSummary("Lista todas as contas")
            .Produces<List<Finance.Domain.Entities.Account>>();

        group.MapGet("/{id:guid}", GetAccountById)
            .WithName("GetAccountById")
            .WithSummary("Busca uma conta por ID")
            .Produces<Finance.Domain.Entities.Account>()
            .Produces(404);
    }

    private static async Task<IResult> GetAllAccounts(IAccountRepository repository)
    {
        var accounts = await repository.GetAllAsync();
        return Results.Ok(accounts);
    }

    private static async Task<IResult> GetAccountById(Guid id, IAccountRepository repository)
    {
        var account = await repository.GetByIdAsync(id);
        return account != null ? Results.Ok(account) : Results.NotFound();
    }
}
