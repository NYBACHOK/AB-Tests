using AbTests.Api.Managers;

namespace AbTests.Api.Extensions;

public static class RouteExtensions
{
    /// <summary>
    /// Add minimal endpoints for this api
    /// </summary>
    /// <returns><see cref="WebApplication"/> for chaining call</returns>
    public static WebApplication AddRoutes(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        
        //create client manually
        app.MapPost("/{deviceToken}",
            async (Guid deviceToken, ApiManager manager) => await manager.AddClient(deviceToken));

        app.MapGet("experiment/button",
            async (Guid deviceToken, ApiManager manager) => await manager.ButtonExperiment(deviceToken));

        app.MapGet("experiment/price", async (Guid deviceToken, ApiManager manager) =>
            await manager.ColorExperiment(deviceToken));

        return app;
    }
}