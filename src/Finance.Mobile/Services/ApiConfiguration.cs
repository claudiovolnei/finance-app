namespace Finance.Mobile.Services;

public static class ApiConfiguration
{
    /// <summary>URL padrão do backend por plataforma (usada quando appsettings não define Api:BaseUrl).</summary>
    public static string GetDefaultBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5102";
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return "http://localhost:5102";
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return "http://localhost:5102";
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return "http://localhost:5102";
        return "http://localhost:5102";
    }
}
