namespace Finance.Mobile.Services;

public class TokenService
{
    private const string TOKEN_KEY = "finance_mobile_token";
    private const string BIOMETRIC_ENABLED_KEY = "finance_mobile_biometric_enabled";
    private const string SAVED_USERNAME_KEY = "finance_mobile_saved_username";
    private static bool _biometricAuthenticatedInSession;

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
        _biometricAuthenticatedInSession = false;
    }

    public void DisableBiometricLogin()
    {
        Preferences.Remove(BIOMETRIC_ENABLED_KEY);
        _biometricAuthenticatedInSession = false;
    }


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

    public bool IsBiometricLoginEnabled() => Preferences.Get(BIOMETRIC_ENABLED_KEY, false);

    public bool RequiresBiometricAuthentication()
    {
        if (!IsBiometricLoginEnabled())
            return false;

        return !_biometricAuthenticatedInSession;
    }

    public void MarkBiometricAuthentication()
    {
        _biometricAuthenticatedInSession = true;
    }


    public void ForceBiometricReauthentication()
    {
        _biometricAuthenticatedInSession = false;
    }


    public async Task<int?> GetUserIdAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var root = DecodeTokenPayload(token);

            if (TryGetIntClaim(root, "nameid", out var userId)
                || TryGetIntClaim(root, "sub", out userId)
                || TryGetIntClaim(root, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetIntClaim(System.Text.Json.JsonElement root, string claimName, out int value)
    {
        value = default;
        if (!root.TryGetProperty(claimName, out var claim))
            return false;

        if (claim.ValueKind == System.Text.Json.JsonValueKind.Number && claim.TryGetInt32(out value))
            return true;

        if (claim.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(claim.GetString(), out value))
            return true;

        return false;
    }

    private static System.Text.Json.JsonElement DecodeTokenPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
            throw new InvalidOperationException("Invalid token");

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    // Decode JWT token (no validation) and return the 'unique name' or Name claim if present
    public async Task<string?> GetUsernameAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var root = DecodeTokenPayload(token);
            string? name = null;
            if (root.TryGetProperty("unique_name", out var p)) name = p.GetString();
            if (string.IsNullOrEmpty(name) && root.TryGetProperty("name", out p)) name = p.GetString();
            if (string.IsNullOrEmpty(name) && root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out p)) name = p.GetString();
            return name;
        }
        catch
        {
            return null;
        }
    }
}
