using Finance.Api.Endpoints;

namespace Finance.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapTransactionEndpoints();
        app.MapCategoryEndpoints();
        app.MapAccountEndpoints();

        return app;
    }
}
