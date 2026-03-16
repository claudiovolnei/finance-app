using Finance.Api.Endpoints.Dtos;
using Finance.Application.Models.OpenFinance;
using Finance.Application.Services;

namespace Finance.Api.Endpoints;

public static class OpenFinanceEndpoints
{
    public static void MapOpenFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/open-finance")
            .WithTags("Open Finance")
            .RequireAuthorization();

        group.MapPost("/connect-account", ConnectBankAccountAsync)
            .WithName("ConnectBankAccount")
            .WithSummary("Connects a bank account using Pluggy")
            .Produces<ConnectBankAccountResponse>()
            .Produces(400);

        group.MapGet("/credit-card-transactions/{accountId}", GetCreditCardTransactionsAsync)
            .WithName("GetCreditCardTransactions")
            .WithSummary("Retrieves credit card transactions from Pluggy")
            .Produces<List<CreditCardTransactionResponse>>()
            .Produces(400);
    }

    private static async Task<IResult> ConnectBankAccountAsync(
        ConnectBankAccountRequest request,
        IPluggyService pluggyService,
        CancellationToken cancellationToken)
    {
        var result = await pluggyService.ConnectBankAccountAsync(
            new ConnectBankAccountRequestModel(
                request.ConnectorId,
                request.ClientUserId,
                request.Parameters,
                request.Products),
            cancellationToken);

        return Results.Ok(new ConnectBankAccountResponse(
            result.ItemId,
            result.ConnectorId,
            result.Status,
            result.ExecutionStatus,
            result.CreatedAt,
            result.ClientUserId));
    }

    private static async Task<IResult> GetCreditCardTransactionsAsync(
        string accountId,
        IPluggyService pluggyService,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var transactions = await pluggyService.GetCreditCardTransactionsAsync(
            accountId,
            from,
            to,
            page,
            pageSize,
            cancellationToken);

        var response = transactions
            .Select(transaction => new CreditCardTransactionResponse(
                transaction.Id,
                transaction.AccountId,
                transaction.Amount,
                transaction.Description,
                transaction.Date,
                transaction.Type,
                transaction.Status,
                transaction.CurrencyCode,
                transaction.MerchantName,
                transaction.Category,
                transaction.ProviderCode))
            .ToList();

        return Results.Ok(response);
    }
}
