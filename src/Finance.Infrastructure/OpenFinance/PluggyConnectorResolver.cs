using Finance.Application.Services;
using Microsoft.Extensions.Options;

namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggyConnectorResolver(IOptions<PluggyOptions> options) : IPluggyConnectorResolver
{
    private readonly PluggyOptions _options = options.Value;

    public string GetSantanderConnectorId()
    {
        if (string.IsNullOrWhiteSpace(_options.SantanderConnectorId))
        {
            throw new InvalidOperationException("Pluggy:SantanderConnectorId is not configured.");
        }

        return _options.SantanderConnectorId;
    }
}
