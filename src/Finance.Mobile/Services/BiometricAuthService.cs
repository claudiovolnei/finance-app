using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Finance.Mobile.Services;

public class BiometricAuthService
{
    private static readonly SemaphoreSlim PromptLock = new(1, 1);
    private readonly ILogger<BiometricAuthService> _logger;

    public BiometricAuthService(ILogger<BiometricAuthService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync()
    {
#if ANDROID || IOS
        try
        {
#if ANDROID
            // Some Android devices report false when requiring enrollment even though
            // the prompt can still be shown. Check both modes for better compatibility.
            var availableWithEnrollment = await CrossFingerprint.Current.IsAvailableAsync(true);
            if (availableWithEnrollment)
                return true;

            return await CrossFingerprint.Current.IsAvailableAsync(false);
#else
            return await CrossFingerprint.Current.IsAvailableAsync(true);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check biometric availability");
            return false;
        }
#else
        await Task.CompletedTask;
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Biometric authentication failed");
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
