using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Finance.Mobile.Services;

namespace Finance.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Configurar HttpClient para comunicação com a API
		var apiBaseUrl = ApiConfiguration.GetBaseUrl();

		builder.Services.AddHttpClient<FinanceApiClient>(client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		builder.Services.AddScoped<FinanceApiClient>();

		builder.Services.AddMudServices(config =>
		{
			config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
			config.SnackbarConfiguration.VisibleStateDuration = 3000;
			config.SnackbarConfiguration.HideTransitionDuration = 200;
			config.SnackbarConfiguration.ShowTransitionDuration = 200;
		});

	#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
	#endif

		return builder.Build();
	}
}
