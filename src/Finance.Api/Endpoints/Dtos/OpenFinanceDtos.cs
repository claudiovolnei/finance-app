namespace Finance.Api.Endpoints.Dtos;

public sealed record ConnectBankAccountRequest(
    string ConnectorId,
    string? ClientUserId,
    Dictionary<string, string> Parameters,
    string[]? Products);

public sealed record ConnectBankAccountResponse(
    string ItemId,
    string ConnectorId,
    string Status,
    string? ExecutionStatus,
    DateTime? CreatedAt,
    string? ClientUserId);

public sealed record CreditCardTransactionResponse(
    string Id,
    string AccountId,
    decimal Amount,
    string Description,
    DateTime Date,
    string Type,
    string Status,
    string CurrencyCode,
    string? MerchantName,
    string? Category,
    string? ProviderCode);
