using System.Net;

namespace Finance.Mobile.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly BiometricAuthService _biometricAuthService;

    public AuthMessageHandler(TokenService tokenService, BiometricAuthService biometricAuthService)
    {
        _tokenService = tokenService;
        _biometricAuthService = biometricAuthService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            if (_tokenService.RequiresBiometricAuthentication())
            {
                var authenticated = await _biometricAuthService.AuthenticateAsync("Confirme sua biometria para continuar");
                if (!authenticated)
                {
                    await _tokenService.RemoveTokenAsync();
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        RequestMessage = request
                    };
                }

                _tokenService.MarkBiometricAuthentication();
            }

            // ensure single Authorization header
            request.Headers.Remove("Authorization");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // remove token when unauthorized; do not navigate from handler (UI will handle redirect)
            await _tokenService.RemoveTokenAsync();
        }

        return response;
    }
}
