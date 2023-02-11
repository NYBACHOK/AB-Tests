using AbTests.Api.Accessors.Interfaces;
using AbTests.Api.DO;
using AbTests.Api.Dto;
using Microsoft.Extensions.Caching.Memory;

namespace AbTests.Api.Accessors;

public class CacheAccessor : ICacheAccessor
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;
    
    public CacheAccessor(IMemoryCache cache)
    {
        _cache = cache;
        _options = new MemoryCacheEntryOptions 
        {
            
            AbsoluteExpirationRelativeToNow = EnvironmentVariables.CacheLifeTime
        };
    }

    public bool TryGetExperiment(Guid deviceToken, string experimentName, out ExperimentResultDto? result)
    {
        try
        {
            var experimentResultDto = _cache.Get<ExperimentResultDto?>(KeysToStr(deviceToken, experimentName));
            if (experimentResultDto is null)
            {
                result = new ExperimentResultDto();
                return false;
            }
                

            result = experimentResultDto;
            return true;
        }
        catch (Exception)
        {
            result = new ExperimentResultDto();
            return false;
        }
    }

    public bool AddToCache(ExperimentResultDto result, Guid deviceToken, string experimentName)
    {
        try
        {
            _cache.Set(KeysToStr(deviceToken, experimentName), result, _options);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string KeysToStr(Guid deviceToken, string experimentName)
    {
        return $"{deviceToken.ToString()}-{experimentName}";
    }
}