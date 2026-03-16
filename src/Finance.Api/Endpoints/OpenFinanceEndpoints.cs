using Finance.Api.Endpoints.Dtos;
using Finance.Application.Models.OpenFinance;
using Finance.Application.Repositories;
using Finance.Application.Services;
using Finance.Domain.Entities;

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

        group.MapPost("/santander/connect-account", ConnectSantanderAccountAsync)
            .WithName("ConnectSantanderAccount")
            .WithSummary("Connects a Santander account using configured Pluggy connector")
            .Produces<ConnectBankAccountResponse>()
            .Produces(400);

        group.MapGet("/credit-card-transactions/{accountId}", GetCreditCardTransactionsAsync)
            .WithName("GetCreditCardTransactions")
            .WithSummary("Retrieves credit card transactions from Pluggy")
            .Produces<List<CreditCardTransactionResponse>>()
            .Produces(400);

        group.MapPost("/santander/import-transactions", ImportSantanderTransactionsAsync)
            .WithName("ImportSantanderTransactions")
            .WithSummary("Imports Santander transactions and suggests category per transaction")
            .Produces<SantanderImportTransactionsResponse>()
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

    private static async Task<IResult> ConnectSantanderAccountAsync(
        HttpContext httpContext,
        ConnectSantanderAccountRequest request,
        IPluggyService pluggyService,
        IPluggyConnectorResolver connectorResolver,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);
        if (userId <= 0)
        {
            return Results.Unauthorized();
        }

        var connectorId = connectorResolver.GetSantanderConnectorId();
        var clientUserId = string.IsNullOrWhiteSpace(request.ClientUserId)
            ? userId.ToString()
            : request.ClientUserId;

        var result = await pluggyService.ConnectBankAccountAsync(
            new ConnectBankAccountRequestModel(
                connectorId,
                clientUserId,
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

    private static async Task<IResult> ImportSantanderTransactionsAsync(
        HttpContext httpContext,
        ImportSantanderTransactionsRequest request,
        IPluggyService pluggyService,
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IOpenFinanceCategorizationService categorizationService,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(httpContext);
        if (userId <= 0)
        {
            return Results.Unauthorized();
        }

        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;

        var openFinanceTransactions = await pluggyService.GetCreditCardTransactionsAsync(
            request.PluggyAccountId,
            from,
            to,
            request.Page,
            request.PageSize,
            cancellationToken);

        var categories = await categoryRepository.GetByUserIdAsync(userId);
        var categorizedTransactions = openFinanceTransactions
            .Select(transaction => categorizationService.SuggestCategory(transaction, categories))
            .ToList();

        var created = 0;
        var skipped = 0;

        if (request.PersistOnImport)
        {
            var existing = await transactionRepository.GetByUserIdAsync(userId, from.Year, from.Month, request.AppAccountId);

            foreach (var item in categorizedTransactions)
            {
                if (AlreadyExists(existing, item.Transaction))
                {
                    skipped++;
                    continue;
                }

                var newTransaction = new Transaction(
                    request.AppAccountId,
                    item.SuggestedCategoryId,
                    null,
                    Math.Abs(item.Transaction.Amount),
                    item.Transaction.Date,
                    item.Transaction.Description,
                    ParseTransactionType(item.Transaction.Type),
                    userId);

                await transactionRepository.AddAsync(newTransaction);
                created++;
            }
        }

        var response = new SantanderImportTransactionsResponse(
            categorizedTransactions.Count,
            created,
            skipped,
            categorizedTransactions.Select(item => new SantanderImportedTransactionResponse(
                item.Transaction.Id,
                item.Transaction.Description,
                item.Transaction.Amount,
                item.Transaction.Date,
                item.Transaction.Type,
                item.Transaction.ProviderCode,
                item.SuggestedCategoryId,
                item.SuggestedCategoryName,
                item.Confidence,
                item.Reason)).ToList());

        return Results.Ok(response);
    }

    private static bool AlreadyExists(IEnumerable<Transaction> existing, CreditCardTransactionModel candidate)
    {
        return existing.Any(t =>
            t.Date.Date == candidate.Date.Date &&
            t.Description.Equals(candidate.Description, StringComparison.OrdinalIgnoreCase) &&
            Math.Abs(t.Amount) == Math.Abs(candidate.Amount));
    }

    private static TransactionType ParseTransactionType(string type)
    {
        if (string.Equals(type, "CREDIT", StringComparison.OrdinalIgnoreCase))
        {
            return TransactionType.Income;
        }

        return TransactionType.Expense;
    }

    private static int GetCurrentUserId(HttpContext httpContext)
    {
        var userClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userClaim is null || !int.TryParse(userClaim.Value, out var userId))
        {
            return 0;
        }

        return userId;
    }
}
