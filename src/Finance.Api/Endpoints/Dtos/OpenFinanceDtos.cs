namespace Finance.Api.Endpoints.Dtos;

public sealed record ConnectBankAccountRequest(
    string ConnectorId,
    string? ClientUserId,
    Dictionary<string, string> Parameters,
    string[]? Products);

public sealed record ConnectSantanderAccountRequest(
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

public sealed record ImportSantanderTransactionsRequest(
    int AppAccountId,
    string PluggyAccountId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 100,
    bool PersistOnImport = true);

public sealed record SantanderImportTransactionsResponse(
    int Retrieved,
    int Imported,
    int Skipped,
    List<SantanderImportedTransactionResponse> Transactions);

public sealed record SantanderImportedTransactionResponse(
    string OpenFinanceTransactionId,
    string Description,
    decimal Amount,
    DateTime Date,
    string Type,
    string? ProviderCode,
    int? SuggestedCategoryId,
    string? SuggestedCategoryName,
    decimal Confidence,
    string Reason);
