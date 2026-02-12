using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.ApplicationModel;

namespace Finance.Mobile.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly NavigationManager _nav;

    public AuthMessageHandler(TokenService tokenService, NavigationManager nav)
    {
        _tokenService = tokenService;
        _nav = nav;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            // ensure single Authorization header
            request.Headers.Remove("Authorization");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // remove token and redirect to login on main thread
            await _tokenService.RemoveTokenAsync();
            try
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    try { _nav.NavigateTo("/login"); } catch { }
                });
            }
            catch
            {
                // ignore navigation errors
            }
        }

        return response;
    }
}
