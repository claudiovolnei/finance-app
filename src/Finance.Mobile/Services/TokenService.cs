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

    // Decode JWT token (no validation) and return the 'unique name' or Name claim if present
    public async Task<string?> GetUsernameAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            // Manual JWT payload decode (base64url) to avoid adding JWT packages on the client
            var parts = token.Split('.');
            if (parts.Length < 2) return null;
            var payload = parts[1];
            // base64url -> base64
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            string? name = null;
            if (root.TryGetProperty("unique_name", out var p)) name = p.GetString();
            if (string.IsNullOrEmpty(name) && root.TryGetProperty("name", out p)) name = p.GetString();
            // common claim URI for name
            if (string.IsNullOrEmpty(name) && root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out p)) name = p.GetString();
            return name;
        }
        catch
        {
            return null;
        }
    }
}
