namespace Finance.Mobile.Services;

public static class ApiConfiguration
{
    /// <summary>URL padrão do backend por plataforma (usada quando appsettings não define Api:BaseUrl).</summary>
    public static string GetDefaultBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "https://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return "https://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return "https://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return "https://my-finance.runasp.net";
        return "https://my-finance.runasp.net";
    }
}
