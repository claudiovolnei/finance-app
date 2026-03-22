using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

public record TransactionDto(
    int Id,
    int AccountId,
    string AccountName,
    AccountType AccountType,
    int? ParentAccountId,
    string? ParentAccountName,
    bool IsCreditCardExpense,
    int? CategoryId,
    int? TransferAccountId,
    string TransactionAccountName,
    string CategoryName,
    decimal Amount,
    DateTime Date,
    string? Description,
    TransactionType Type);
