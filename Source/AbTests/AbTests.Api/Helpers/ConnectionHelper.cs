using Npgsql;
using Polly;
using Polly.Retry;

namespace AbTests.Api.Helpers;

public static class ConnectionHelper
{
    private static readonly RetryPolicy<NpgsqlConnection>  _retryPolicy;
    
    private const int RETRY_COUNT = 5;

    static ConnectionHelper()
    {
        _retryPolicy = Polly.Policy<NpgsqlConnection>
            .Handle<NpgsqlException>()
            .WaitAndRetry(RETRY_COUNT, retryAttempt =>
            {
                var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                Console.WriteLine($"{retryAttempt} attempt failed. Waiting {timeToWait.TotalSeconds} seconds");
                return timeToWait;
            });
    }
    
    public static NpgsqlConnection StartConnection(string connectionString)
    {
        return _retryPolicy.Execute(() =>
        {
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        });
    } 
}