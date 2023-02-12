﻿using AbTests.Api.DO;
using AbTests.Api.Dto;
using AbTests.Api.Helpers;
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
        app.MapGet("experiments/list", async (ApiManager manager) => ResponseHelper.SendResponse(await manager.GetExperiments()))
            .Produces<Response<List<Experiment>>>();
        
        app.MapGet("experiment/statistic", async (ApiManager manager) => ResponseHelper.SendResponse(await manager.GetStatistic()))
            .Produces<Response<List<Statistic>>>();

        //Do experiment based on input
        app.MapPost("experiments/submit",
            async (Guid deviceToken, string experimentName, ApiManager manager) =>
                ResponseHelper.SendResponse(await manager.GetExperimentResult(deviceToken, experimentName)))
            .Produces<Response<ExperimentResultDto>>();

        return app;
    }
}