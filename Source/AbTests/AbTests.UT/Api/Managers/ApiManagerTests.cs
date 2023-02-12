using AbTests.Api.Accessors.Interfaces;
using AbTests.Api.DO;
using AbTests.Api.Dto;
using AbTests.Api.Enums;
using AbTests.Api.Helpers.Interfaces;
using AbTests.Api.Managers;
using FluentAssertions;
using Moq;
using Npgsql;

namespace AbTests.UT.Api.Managers;

public class ApiManagerTests
{
    private readonly ApiManager _manager;

    private Mock<ISqlAccessor> _sqlAccessorMock;
    private Mock<IRandomHelper> _randomHelperMock;
    private Mock<ICacheAccessor> _cacheAccessorMock;

    private const ResponseCode NO_ERROR = ResponseCode.NoError;
    private const ResponseCode SQL_EXCEPTION = ResponseCode.SqlException;
    private const ResponseCode UNKNOWN_ERROR = ResponseCode.UnknownError;
    private const ResponseCode INVALID_MODEL = ResponseCode.InvalidModel;
    
    public ApiManagerTests()
    {
        _sqlAccessorMock = new Mock<ISqlAccessor>();
        _randomHelperMock = new Mock<IRandomHelper>();
        _cacheAccessorMock = new Mock<ICacheAccessor>();

        _manager = new ApiManager(_sqlAccessorMock.Object, _randomHelperMock.Object, _cacheAccessorMock.Object);
    }

    #region GetExperiments

    [Fact]
    public async Task GetExperiments_OK()
    {
        _sqlAccessorMock.Setup(_ => _.GetExperiments())
            .ReturnsAsync(new List<Experiment> { new Experiment { ExpId = 1, ExpName = "name" } });
        
        //
        var result = await _manager.GetExperiments();
        
        //
        Assert.NotNull(result);
        Assert.NotNull(result.Result);
        Assert.Equal(NO_ERROR, result.Error);
        Assert.Single(result.Result!);
    }
    
    [Fact]
    public async Task GetExperiments_Npgsql_Exception()
    {
        _sqlAccessorMock.Setup(_ => _.GetExperiments())
            .ThrowsAsync(new NpgsqlException());
        
        //
        var result = await _manager.GetExperiments();
        
        //
        Assert.NotNull(result);
        Assert.Equal(SQL_EXCEPTION, result.Error);
    }
    
    [Fact]
    public async Task GetExperiments_Exception()
    {
        _sqlAccessorMock.Setup(_ => _.GetExperiments())
            .ThrowsAsync(new Exception());
        
        //
        var result = await _manager.GetExperiments();
        
        //
        Assert.NotNull(result);
        Assert.Equal(UNKNOWN_ERROR, result.Error);
    }

    #endregion GetExperiments

    #region GetStatistic

    [Fact]
    public async Task GetStatistic_OK()
    {
        _sqlAccessorMock.Setup(_ => _.GetStatistic())
            .ReturnsAsync(new List<Statistic>{ new() { }});
        
        //
        var result = await _manager.GetStatistic();
        
        //
        Assert.NotNull(result);
        Assert.Equal(NO_ERROR, result.Error);
        Assert.Single(result.Result!);
    }
    
    [Fact]
    public async Task GetStatistic_Npgsql_Exception()
    {
        _sqlAccessorMock.Setup(_ => _.GetStatistic())
            .ThrowsAsync(new NpgsqlException());
        
        //
        var result = await _manager.GetStatistic();
        
        //
        Assert.NotNull(result);
        Assert.Equal(SQL_EXCEPTION, result.Error);
    }
    
    [Fact]
    public async Task GetStatistic_Exception()
    {
        _sqlAccessorMock.Setup(_ => _.GetStatistic())
            .ThrowsAsync(new Exception());
        
        //
        var result = await _manager.GetStatistic();
        
        //
        Assert.NotNull(result);
        Assert.Equal(UNKNOWN_ERROR, result.Error);
    }

    #endregion GetStatistic

    #region GetExperimentResult

    [Fact]
    public async Task GetExperimentResult_InvalidModel_DeviceToken()
    {
        var deviceToken = Guid.Empty;
        string experimentName = "SomeName";
        
        //
        var result = await _manager.GetExperimentResult(deviceToken, experimentName);
        
        //
        Assert.NotNull(result);
        Assert.Equal(INVALID_MODEL, result.Error);
    }
    
    [Fact]
    public async Task GetExperimentResult_InvalidModel_ExpName()
    {
        var deviceToken = Guid.NewGuid();
        string experimentName = string.Empty;
        
        //
        var result = await _manager.GetExperimentResult(deviceToken, experimentName);
        
        //
        Assert.NotNull(result);
        Assert.Equal(INVALID_MODEL, result.Error);
    }

    [Fact]
    public async Task GetExperimentResult_FromCache()
    {
        var deviceToken = Guid.NewGuid();
        string experimentName = "SomeValue";
        
        ExperimentResultDto? resultDto = new ExperimentResultDto
        {
            Key = "SomeKey",
            Value = "SomeValue"
        };

        _cacheAccessorMock.Setup(_ => _.TryGetExperiment(It.IsAny<Guid>(), It.IsAny<string>(), out resultDto))
            .Returns(true);
        //
        var result = await _manager.GetExperimentResult(deviceToken, experimentName);
        
        //
        Assert.Equal(NO_ERROR, result.Error);
        result.Result.Should()
            .BeEquivalentTo(resultDto);
    }

    [Fact]
    public async Task GetExperimentResult_FailedToCreateClient()
    {
        var deviceToken = Guid.NewGuid();
        string experimentName = "SomeValue";
        
        ExperimentResultDto? resultDto = new ExperimentResultDto
        {
            Key = "SomeKey",
            Value = "SomeValue"
        };

        _cacheAccessorMock.Setup(_ => _.TryGetExperiment(It.IsAny<Guid>(), It.IsAny<string>(), out resultDto))
            .Returns(false);

        _sqlAccessorMock.Setup(_ => _.AddClient(It.IsAny<Guid>()))
            .ReturnsAsync(null as Client);
        
        _sqlAccessorMock.Setup(_ => _.GetClient(It.IsAny<Guid>()))
            .ReturnsAsync(null as Client);
        
        //
        var result = await _manager.GetExperimentResult(deviceToken, experimentName);

        //
        Assert.Equal(SQL_EXCEPTION, result.Error);
    }

    #endregion GetExperimentResult
}