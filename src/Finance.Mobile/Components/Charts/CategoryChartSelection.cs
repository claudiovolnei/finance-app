using Finance.Domain.Entities;

namespace Finance.Mobile.Components.Charts;

public readonly record struct CategoryChartSelection(int? CategoryId, string Label, TransactionType Type);
