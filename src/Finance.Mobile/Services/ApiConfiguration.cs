namespace Finance.Mobile.Services;

public static class ApiConfiguration
{
    public static string GetBaseUrl()
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
