using Finance.Domain.Entities;

namespace Finance.Api.Endpoints.Dtos;

public record CategorySummaryDto(int? CategoryId, string Name, decimal Amount, decimal Percentage, TransactionType Type);

public record CreditCardExpenseSummaryDto(int AccountId, string AccountName, decimal Amount);

public record TransactionSummaryDto(
    int Id,
    int AccountId,
    string AccountName,
    AccountType AccountType,
    int? ParentAccountId,
    string? ParentAccountName,
    bool IsCreditCardExpense,
    int? CategoryId,
    string CategoryName,
    decimal Amount,
    DateTime Date,
    string Description,
    TransactionType Type,
    int? TransferAccountId,
    string TransactionAccountName);

public record CategoryExpenseDetailDto(int CategoryId, string CategoryName, decimal TotalExpense, List<TransactionSummaryDto> Transactions);

public record CategoryTransactionsDetailDto(int? CategoryId, string Title, decimal TotalAmount, TransactionType Type, List<TransactionSummaryDto> Transactions);

public record DashboardSummaryDto(
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal TotalCreditCardDebt,
    List<CategorySummaryDto> Categories,
    List<CreditCardExpenseSummaryDto> CreditCardExpenses,
    List<TransactionSummaryDto> LatestTransactions);
