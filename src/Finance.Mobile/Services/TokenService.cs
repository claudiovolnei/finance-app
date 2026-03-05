namespace Finance.Mobile.Services;

public class TokenService
{
    private const string TOKEN_KEY = "finance_mobile_token";
    private const string BIOMETRIC_ENABLED_KEY = "finance_mobile_biometric_enabled";
    private const string BIOMETRIC_LAST_AUTH_UTC_KEY = "finance_mobile_biometric_last_auth_utc";
    private const string BIOMETRIC_PERMISSION_REQUESTED_KEY = "finance_mobile_biometric_permission_requested";
    private const string SAVED_USERNAME_KEY = "finance_mobile_saved_username";
    private const int BIOMETRIC_EXPIRATION_MINUTES = 10;

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

        DisableBiometricLogin();
        RemoveSavedUsername();
        await Task.CompletedTask;
    }

    public void EnableBiometricLogin()
    {
        Preferences.Set(BIOMETRIC_ENABLED_KEY, true);
        Preferences.Set(BIOMETRIC_PERMISSION_REQUESTED_KEY, true);
        MarkBiometricAuthentication();
    }

    public void DisableBiometricLogin()
    {
        Preferences.Remove(BIOMETRIC_ENABLED_KEY);
        Preferences.Remove(BIOMETRIC_LAST_AUTH_UTC_KEY);
    }

    public bool HasRequestedBiometricPermission() => Preferences.Get(BIOMETRIC_PERMISSION_REQUESTED_KEY, false);


    public void SaveUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return;

        Preferences.Set(SAVED_USERNAME_KEY, username.Trim());
    }

    public string? GetSavedUsername() => Preferences.Get(SAVED_USERNAME_KEY, null as string);

    public void RemoveSavedUsername() => Preferences.Remove(SAVED_USERNAME_KEY);

    public bool MatchesSavedUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var savedUsername = GetSavedUsername();
        if (string.IsNullOrWhiteSpace(savedUsername))
            return false;

        return string.Equals(savedUsername.Trim(), username.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void MarkBiometricPermissionRequested() => Preferences.Set(BIOMETRIC_PERMISSION_REQUESTED_KEY, true);

    public bool IsBiometricLoginEnabled() => Preferences.Get(BIOMETRIC_ENABLED_KEY, false);

    public bool RequiresBiometricAuthentication()
    {
        if (!IsBiometricLoginEnabled())
            return false;

        var lastAuthRaw = Preferences.Get(BIOMETRIC_LAST_AUTH_UTC_KEY, null as string);
        if (string.IsNullOrWhiteSpace(lastAuthRaw) || !DateTimeOffset.TryParse(lastAuthRaw, out var lastAuthUtc))
            return true;

        return DateTimeOffset.UtcNow - lastAuthUtc > TimeSpan.FromMinutes(BIOMETRIC_EXPIRATION_MINUTES);
    }

    public void MarkBiometricAuthentication()
    {
        Preferences.Set(BIOMETRIC_LAST_AUTH_UTC_KEY, DateTimeOffset.UtcNow.ToString("O"));
    }

    public int GetBiometricExpirationMinutes() => BIOMETRIC_EXPIRATION_MINUTES;


    public void ForceBiometricReauthentication()
    {
        Preferences.Remove(BIOMETRIC_LAST_AUTH_UTC_KEY);
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
