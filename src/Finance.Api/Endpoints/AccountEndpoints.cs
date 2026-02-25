using Finance.Application.Repositories;

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
            .Produces<List<Finance.Domain.Entities.Account>>();

        group.MapGet("/{id:int}", GetAccountById)
            .WithName("GetAccountById")
            .WithSummary("Busca uma conta por ID")
            .Produces<Finance.Domain.Entities.Account>()
            .Produces(404);
    }

    private static async Task<IResult> GetAllAccounts(HttpContext httpContext, IAccountRepository repository)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        var accounts = await repository.GetByUserIdAsync(userId);
        return Results.Ok(accounts);
    }

    private static async Task<IResult> GetAccountById(HttpContext httpContext, int id, IAccountRepository repository)
    {
        var account = await repository.GetByIdAsync(id);
        if (account == null) return Results.NotFound();

        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int userId = 0;
        if (userClaim != null && int.TryParse(userClaim.Value, out var parsed))
            userId = parsed;

        if (account.UserId != userId)
            return Results.Forbid();

        return Results.Ok(account);
    }
}
