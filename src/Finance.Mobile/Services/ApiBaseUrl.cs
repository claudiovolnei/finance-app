namespace Finance.Mobile.Services;

public sealed class ApiBaseUrl
{
    public ApiBaseUrl(string baseUrl)
    {
        Value = baseUrl?.TrimEnd('/') ?? "";
    }

    public string Value { get; }

    public string GetAbsoluteUrl(string path) =>
        string.IsNullOrEmpty(Value)
            ? throw new InvalidOperationException("ApiBaseUrl não configurado.")
            : $"{Value}/{path.TrimStart('/')}";
}
