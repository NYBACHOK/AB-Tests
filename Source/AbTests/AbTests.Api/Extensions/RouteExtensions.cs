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
        
        //Get available list of experiments
        app.MapGet("experiments/list", async (ApiManager manager) => await manager.GetExperiments());
        
        app.MapGet("experiment/statistic", async (ApiManager manager) => await manager.GetStatistic());

        //Do experiment based on input
        app.MapPost("experiments/submit",
            async (Guid deviceToken, string experimentName, ApiManager manager) =>
                await manager.GetExperimentResult(deviceToken, experimentName));

        return app;
    }
}