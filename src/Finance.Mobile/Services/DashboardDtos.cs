using Finance.Domain.Entities;

namespace Finance.Mobile.Services;

// DTOs matching API Dashboard response
public record CategorySummaryDto(Guid CategoryId, string Name, decimal Amount, decimal Percentage);
public record TransactionSummaryDto(Guid Id, DateTime Date, string Description, Guid CategoryId, string CategoryName, decimal Amount, TransactionType Type);
public record DashboardSummaryDto(decimal Balance, decimal TotalIncome, decimal TotalExpense, List<CategorySummaryDto> Categories, List<TransactionSummaryDto> LatestTransactions);
