using Microsoft.Maui.Storage;
namespace Finance.Mobile.Services;

public class TokenService
{
    private const string TOKEN_KEY = "finance_mobile_token";

    public async Task SaveTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        try
        {
            await SecureStorage.Default.SetAsync(TOKEN_KEY, token);
        }
        catch
        {
            // fallback to Preferences
            Preferences.Set(TOKEN_KEY, token);
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var v = await SecureStorage.Default.GetAsync(TOKEN_KEY);
            if (!string.IsNullOrEmpty(v)) return v;
        }
        catch
        {
            // ignore
        }
        return Preferences.Get(TOKEN_KEY, null as string);
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            SecureStorage.Default.Remove(TOKEN_KEY);
        }
        catch
        {
            Preferences.Remove(TOKEN_KEY);
        }
        await Task.CompletedTask;
    }
}
