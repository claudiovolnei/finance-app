namespace Finance.Application.Models.OpenFinance;

public sealed record CreditCardTransactionModel(
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
