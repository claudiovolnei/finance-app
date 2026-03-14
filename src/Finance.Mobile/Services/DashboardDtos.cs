using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

// DTOs matching API Dashboard response
public record CategorySummaryDto(int? CategoryId, string Name, decimal Amount, decimal Percentage, TransactionType Type);
public record TransactionSummaryDto(int Id, DateTime Date, string Description, int? CategoryId, string CategoryName, decimal Amount, TransactionType Type, string TransactionAccountName);
public record CategoryExpenseDetailDto(int CategoryId, string CategoryName, decimal TotalExpense, List<TransactionSummaryDto> Transactions);
public record DashboardSummaryDto(decimal Balance, decimal TotalIncome, decimal TotalExpense, List<CategorySummaryDto> Categories, List<TransactionSummaryDto> LatestTransactions);
