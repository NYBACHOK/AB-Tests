using System.Runtime.InteropServices.ComTypes;
using AbTests.Api.DO;
using AbTests.Api.Dto;
using AbTests.Api.Enums;

namespace AbTests.Api.Helpers;

public class ResponseHelper
{
    public static IResult SendResponse<T>(Response<T> response)
    {
        return response.Error switch
        {
            ResponseCode.NoError => Results.Ok(response),
            ResponseCode.InvalidModel => Results.BadRequest(response),
            ResponseCode.SqlException => Results.BadRequest(response),
            ResponseCode.ErrorInExperiment => Results.BadRequest(response),
            _=> Results.Problem()
        };
    }
}