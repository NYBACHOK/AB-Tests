namespace AbTests.Api;

public class EnvironmentVariables
{
    public static readonly string DbConnectionString =
        Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING") ?? throw new Exception();
}