using AbTests.Api.Acessors;
using AbTests.Api.DO;
using AbTests.Api.Dto;
using AbTests.Api.Enums;
using Npgsql;

namespace AbTests.Api.Managers;

public class ApiManager
{
    private readonly SqlAccessor _accessor;
    
    public ApiManager(SqlAccessor accessor)
    {
        _accessor = accessor;
    }

    public async Task<Response<bool>> AddClient(Guid deviceToken)
    {
        try
        {
            var result = await _accessor.AddClient(deviceToken);
            return FormatResponse(result is not null);
        }
        catch (NpgsqlException)
        {
            return FormatResponse<bool>(ResponseCode.SqlException);
        }
        catch (Exception)
        {
            return FormatResponse<bool>(ResponseCode.UnknownError);
        }
    }

    public async Task<Response<ExperimentResultDto>> ButtonExperiment(Guid deviceToken)
    {
        try
        {
            var client = await _accessor.GetClient(deviceToken) 
                         ?? await _accessor.AddClient(deviceToken) 
                         ?? throw new NpgsqlException();

            var rnd = new Random();
            int randomResult = rnd.Next(3);

            //hard code for button_experiment
            var experimentValues = await _accessor.GetExperimentValues(experimentId: 1);
            experimentValues = experimentValues.OrderBy(_ => _.OptionName).ToList();
            
            
            var experimentResult = await _accessor.GetClientExperiment(client.ClientId, 1);
            if(experimentResult is not null)
                return FormatResponse(new ExperimentResultDto { Key = "button_color", Value = experimentValues
                    .Single(_=> _.ExampleId == experimentResult.ExampleId).OptionName }); 
            
            var value = randomResult switch
            {
                0 => experimentValues[0],
                1 => experimentValues[1],
                2 => experimentValues[2],
                _=> throw new InvalidOperationException()
            };

            var saveResult = await _accessor.SaveExperimentResult(client.ClientId, value.ExampleId);
            if(!saveResult)
                return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException); 

            return FormatResponse(new ExperimentResultDto { Key = "button_color", Value = value.OptionName });
        }
        catch (NpgsqlException)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException);
        }
        catch (Exception)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.UnknownError);
        }
    }

    public async Task<Response<ExperimentResultDto>> ColorExperiment(Guid deviceToken)
    {
        try
        {
            var client = await _accessor.GetClient(deviceToken) 
                         ?? await _accessor.AddClient(deviceToken) 
                         ?? throw new NpgsqlException();

            var rnd = new Random();
            int randomResult = rnd.Next(100);

            //hard code for color
            var experimentValues = await _accessor.GetExperimentValues(experimentId: 2);
            
            var experimentResult = await _accessor.GetClientExperiment(client.ClientId, 2);
            if(experimentResult is not null)
                return FormatResponse(new ExperimentResultDto { Key = "price", Value = experimentValues
                    .Single(_=> _.ExampleId == experimentResult.ExampleId).OptionName });

            ExperimentExample? value = null;
            if (Enumerable.Range(0, 75).Contains(randomResult))
                value = experimentValues.Single(_ => _.OptionName == "10");
            else if(Enumerable.Range(76, 10).Contains(randomResult))
                value = experimentValues.Single(_ => _.OptionName == "20");
            else if (Enumerable.Range(87, 5).Contains(randomResult))
                value = experimentValues.Single(_ => _.OptionName == "50");
            else
                value = experimentValues.Single(_ => _.OptionName == "5");

            var saveResult = await _accessor.SaveExperimentResult(client.ClientId, value.ExampleId);
            if(!saveResult)
                return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException); 

            return FormatResponse(new ExperimentResultDto { Key = "price", Value = value.OptionName });
        }
        catch (NpgsqlException)
        {
            return FormatResponse<ExperimentResultDto>(ResponseCode.SqlException);
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