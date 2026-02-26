using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

// DTOs matching API Dashboard response
public record CategorySummaryDto(int CategoryId, string Name, decimal Amount, decimal Percentage, TransactionType Type);
public record TransactionSummaryDto(int Id, DateTime Date, string Description, int CategoryId, string CategoryName, decimal Amount, TransactionType Type);
public record DashboardSummaryDto(decimal Balance, decimal TotalIncome, decimal TotalExpense, List<CategorySummaryDto> Categories, List<TransactionSummaryDto> LatestTransactions);
