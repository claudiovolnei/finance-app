namespace Finance.Application.Models.OpenFinance;

public sealed record CategorizedOpenFinanceTransactionModel(
    CreditCardTransactionModel Transaction,
    int? SuggestedCategoryId,
    string? SuggestedCategoryName,
    decimal Confidence,
    string Reason);
