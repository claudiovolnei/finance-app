using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Finance.Mobile;

public partial class MainPage : ContentPage
{
    private bool _rootComponentAttached;
    private const int SplashDelayMs = 2500;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        if (_rootComponentAttached)
        {
            return;
        }

        await Task.Delay(SplashDelayMs);

        blazorWebView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Components.Routes)
        });

        _rootComponentAttached = true;
        Loaded -= OnPageLoaded;
    }
}
