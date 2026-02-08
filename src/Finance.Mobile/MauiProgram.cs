using Microsoft.Extensions.Configuration;
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

		// Carregar URL do backend: appsettings.json (Api:BaseUrl) ou valor padrão por plataforma
		var config = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.Build();

		var apiBaseUrl = config["Api:BaseUrl"]?.Trim();
		if (string.IsNullOrEmpty(apiBaseUrl))
			apiBaseUrl = ApiConfiguration.GetDefaultBaseUrl();
		apiBaseUrl = apiBaseUrl.TrimEnd('/');

		// Registrar a URL base para o client construir URIs absolutos (evita problema de BaseAddress no Blazor)
		builder.Services.AddSingleton(new ApiBaseUrl(apiBaseUrl));
		builder.Services.AddHttpClient<FinanceApiClient>(client =>
		{
			client.Timeout = TimeSpan.FromSeconds(30);
		});

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
