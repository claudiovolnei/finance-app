using System.Net.Http.Json;
using Finance.Application.Services;
using Microsoft.Extensions.Options;

namespace Finance.Infrastructure.OpenFinance;

public sealed class PluggyAuthService(
    IHttpClientFactory httpClientFactory,
    IOptions<PluggyOptions> options) : IPluggyAuthService
{
    private readonly PluggyOptions _options = options.Value;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private string? _cachedApiKey;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetApiKeyAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_cachedApiKey) && DateTimeOffset.UtcNow < _expiresAt)
        {
            return _cachedApiKey;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_cachedApiKey) && DateTimeOffset.UtcNow < _expiresAt)
            {
                return _cachedApiKey;
            }

            if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                throw new InvalidOperationException("Pluggy credentials are not configured.");
            }

            var client = httpClientFactory.CreateClient("PluggyAuth");

            var response = await client.PostAsJsonAsync("/auth", new
            {
                clientId = _options.ClientId,
                clientSecret = _options.ClientSecret
            }, cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Pluggy auth response was empty.");

            _cachedApiKey = payload.ApiKey;
            _expiresAt = DateTimeOffset.UtcNow.AddMinutes(25);

            return _cachedApiKey;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private sealed record AuthResponse(string ApiKey);
}
