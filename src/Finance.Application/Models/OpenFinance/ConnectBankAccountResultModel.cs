namespace Finance.Application.Models.OpenFinance;

public sealed record ConnectBankAccountResultModel(
    string ItemId,
    string ConnectorId,
    string Status,
    string? ExecutionStatus,
    DateTime? CreatedAt,
    string? ClientUserId);
