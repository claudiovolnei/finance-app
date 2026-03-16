namespace Finance.Application.Services;

public interface IPluggyAuthService
{
    Task<string> GetApiKeyAsync(CancellationToken cancellationToken = default);
}
