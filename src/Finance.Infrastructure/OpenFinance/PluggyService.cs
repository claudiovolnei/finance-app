using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Finance.Application.Models.OpenFinance;
using Finance.Application.Services;

namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggyService(
    IHttpClientFactory httpClientFactory,
    IPluggyAuthService pluggyAuthService) : IPluggyService
{
    public async Task<ConnectBankAccountResultModel> ConnectBankAccountAsync(
        ConnectBankAccountRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAuthenticatedClientAsync(cancellationToken);

        var response = await client.PostAsJsonAsync("/items", new
        {
            connectorId = int.Parse(request.ConnectorId, CultureInfo.InvariantCulture),
            parameters = request.Parameters,
            products = request.Products ?? ["ACCOUNTS", "CREDIT_CARDS"],
            clientUserId = request.ClientUserId
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ConnectItemResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Pluggy item response was empty.");

        return new ConnectBankAccountResultModel(
            payload.Id,
            payload.Connector.Id,
            payload.Status,
            payload.ExecutionStatus,
            payload.CreatedAt,
            payload.ClientUserId);
    }

    public async Task<IReadOnlyList<CreditCardTransactionModel>> GetCreditCardTransactionsAsync(
        string accountId,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAuthenticatedClientAsync(cancellationToken);

        var query = new List<string>
        {
            $"accountId={Uri.EscapeDataString(accountId)}",
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (from.HasValue)
        {
            query.Add($"from={from.Value:O}");
        }

        if (to.HasValue)
        {
            query.Add($"to={to.Value:O}");
        }

        var response = await client.GetAsync($"/transactions?{string.Join("&", query)}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TransactionListResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Pluggy transactions response was empty.");

        return payload.Results
            .Where(transaction => transaction.CreditCardMetadata is not null)
            .Select(transaction => new CreditCardTransactionModel(
                transaction.Id,
                transaction.AccountId,
                transaction.Amount,
                transaction.Description,
                transaction.Date,
                transaction.Type,
                transaction.Status,
                transaction.CurrencyCode,
                transaction.Merchant?.Name,
                transaction.Category,
                transaction.ProviderCode))
            .ToList();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(CancellationToken cancellationToken)
    {
        var apiKey = await pluggyAuthService.GetApiKeyAsync(cancellationToken);
        var client = httpClientFactory.CreateClient("PluggyApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private sealed record ConnectItemResponse(
        string Id,
        Connector Connector,
        string Status,
        string? ExecutionStatus,
        DateTime? CreatedAt,
        string? ClientUserId);

    private sealed record Connector(string Id);

    private sealed record TransactionListResponse(IReadOnlyList<TransactionResponse> Results);

    private sealed record TransactionResponse(
        string Id,
        string AccountId,
        decimal Amount,
        string Description,
        DateTime Date,
        string Type,
        string Status,
        string CurrencyCode,
        MerchantResponse? Merchant,
        string? Category,
        string? ProviderCode,
        object? CreditCardMetadata);

    private sealed record MerchantResponse(string? Name);
}
