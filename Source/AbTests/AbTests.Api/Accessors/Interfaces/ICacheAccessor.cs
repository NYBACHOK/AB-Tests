using AbTests.Api.Dto;

namespace AbTests.Api.Accessors.Interfaces;

public interface ICacheAccessor
{
    bool TryGetExperiment(Guid deviceToken, string experimentName, out ExperimentResultDto? result);

    bool AddToCache(ExperimentResultDto result, Guid deviceToken, string experimentName);
}