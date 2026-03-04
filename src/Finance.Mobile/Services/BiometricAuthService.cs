using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Finance.Mobile.Services;

public class BiometricAuthService
{
    private static readonly SemaphoreSlim PromptLock = new(1, 1);

    public async Task<bool> IsAvailableAsync()
    {
#if ANDROID || IOS
        return await CrossFingerprint.Current.IsAvailableAsync(true);
#else
        return false;
#endif
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
#if ANDROID || IOS
        await PromptLock.WaitAsync();
        try
        {
            var request = new AuthenticationRequestConfiguration("Autenticação biométrica", reason)
            {
                AllowAlternativeAuthentication = false
            };
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            return result.Authenticated;
        }
        catch
        {
            return false;
        }
        finally
        {
            PromptLock.Release();
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }
}
