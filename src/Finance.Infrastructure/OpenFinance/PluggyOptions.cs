namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggyOptions
{
    public const string SectionName = "Pluggy";

    public string BaseUrl { get; init; } = "https://api.pluggy.ai";
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string? DefaultCreditCardAccountId { get; init; }
    public string? SantanderConnectorId { get; init; }
}
