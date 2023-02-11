using AbTests.Api.Acessors;
using AbTests.Api.Acessors.Interfaces;
using AbTests.Api.DO;
using AbTests.Api.Dto;
using AbTests.Api.Enums;
using AbTests.Api.Helpers.Interfaces;
using Npgsql;

namespace AbTests.Api.Managers;

public class ApiManager
{
    private readonly ISqlAccessor _accessor;
    private readonly IRandomHelper _random;
    
    public ApiManager(ISqlAccessor accessor, IRandomHelper random)
    {
        _accessor = accessor;
        _random = random;
    }
    
    public async Task<Response<List<Experiment>>> GetExperiments()
    {
        try
        {
            var result = await _accessor.GetExperiments();
            return FormatResponse(result);
        }
        catch (NpgsqlException)
        {
            return FormatResponse<List<Experiment>>(ResponseCode.SqlException);
        }
        catch (Exception)
        {
            return FormatResponse<List<Experiment>>(ResponseCode.UnknownError);
        }
    }

    public async Task<Response<ExperimentResultDto>> GetExperimentResult(Guid deviceToken, string experimentName)
    {
        try
        {
            var client = await _accessor.GetClient(deviceToken)
                         ?? await _accessor.AddClient(deviceToken)
                         ?? throw new NpgsqlException();

            var experimentDefinition = (await _accessor.GetExperiments()).Single(_ => _.ExpName == experimentName);
            var experimentResult = await _accessor.GetClientExperiment(client.ClientId, experimentDefinition.ExpId);
            var experimentsValues = (await _accessor.GetExperimentValues(experimentDefinition.ExpId))
                .OrderBy(_ => _.OptionName).ToList();

            if (experimentResult is not null)
                return FormatResponse(new ExperimentResultDto
                {
                    Value = experimentsValues.Single(_ => _.ExampleId == experimentResult.ExampleId).OptionName,
                    Key = experimentDefinition.ExpName
                });

            var maxValue = experimentsValues.Sum(_ => _.OptionValue);
            int randomResult = _random.Next(maxValue);

            ExperimentExample? result = null;

            int page = 0;
            foreach (var value in experimentsValues)
            {
                if (Enumerable.Range(page, value.OptionValue).Contains(randomResult))
                {
                    result = value;
                    break;
                }

                page += value.OptionValue;
            }

            ArgumentNullException.ThrowIfNull(result);

            var saveResult = await _accessor.SaveExperimentResult(client.ClientId, result.ExampleId);
            if (!saveResult)
                return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException);

            return FormatResponse(new ExperimentResultDto
            {
                Value = result.OptionName,
                Key = experimentDefinition.ExpName
            });
        }
        catch (NpgsqlException)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException);
        }
        catch (ArgumentNullException)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.ErrorInExperiment);
        }
        catch (Exception)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.UnknownError);
        }
    } 

    private Response<T> FormatResponse<T>(T result)
    {
        return new Response<T>
        {
            Error = ResponseCode.NoError,
            Result = result
        };
    }

    private Response<T> FormatResponse<T>(ResponseCode code)
    {
        return code switch
        {
            _ => new Response<T>{ Error = code}
        };
    }
}