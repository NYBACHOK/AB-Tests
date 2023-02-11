using AbTests.Api.Enums;

namespace AbTests.Api.Dto;

public class Response<T>
{
    public T? Result { get; set; }
    public ResponseCode Error { get; set; }
}