namespace Finance.Mobile.Services;

public static class ApiConfiguration
{
    /// <summary>URL padrão do backend por plataforma (usada quando appsettings não define Api:BaseUrl).</summary>
    public static string GetDefaultBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return "http://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return "http://my-finance.runasp.net";
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return "http://my-finance.runasp.net";
        return "http://my-finance.runasp.net";
    }
}
