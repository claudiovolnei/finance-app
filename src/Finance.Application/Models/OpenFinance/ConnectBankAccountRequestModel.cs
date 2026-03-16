namespace Finance.Application.Models.OpenFinance;

public sealed record ConnectBankAccountRequestModel(
    string ConnectorId,
    string? ClientUserId,
    Dictionary<string, string> Parameters,
    string[]? Products);
