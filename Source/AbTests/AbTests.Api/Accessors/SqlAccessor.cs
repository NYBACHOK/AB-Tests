using AbTests.Api.Accessors.Interfaces;
using AbTests.Api.DO;
using AbTests.Api.Helpers;
using Dapper;
using Dapper.Transaction;
using Npgsql;

namespace AbTests.Api.Accessors;

public class SqlAccessor : ISqlAccessor
{
    private readonly string _connectionString;
    
    public SqlAccessor(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Client?> AddClient(Guid deviceToken)
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string query = $"INSERT INTO clients ( devicetoken) VALUES ('{deviceToken}') RETURNING *;";

            var result = await transaction.QuerySingleOrDefaultAsync<Client?>(query);

            return result;
        }
        catch (NpgsqlException)
        {
            await transaction.RollbackAsync();
            return null;
        }
        finally
        {
            await transaction.CommitAsync();
        }
    }

    public async Task<Client?> GetClient(Guid deviceToken)
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);

        try
        {
            string query = $"SELECT * FROM clients WHERE devicetoken = '{deviceToken}';";

            var result = await connection.QuerySingleOrDefaultAsync<Client?>(query);

            return result;
        }
        catch (NpgsqlException)
        {
            return null;
        }
    }

    public async Task<List<Experiment>> GetExperiments()
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);

        string query = "SELECT * FROM experiments;";

        var result = await connection.QueryAsync<Experiment>(query);

        return result.AsList();
    }

    public async Task<List<ExperimentExample>> GetExperimentValues(int experimentId)
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);

        string query = $"SELECT * FROM experimentexamples WHERE expid = {experimentId};";

        var result = await connection.QueryAsync<ExperimentExample>(query);

        return result.AsList();
    }

    public async Task<ExperimentResult?> GetClientExperiment(int clientId, int experimentId)
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);

        string query =
            "SELECT * FROM clientexperimentresult WHERE clientid = @clientId AND exampleid IN (SELECT ex.exampleid FROM experimentexamples ex WHERE ex.expid = @experimentId);";

        var result =
            await connection.QuerySingleOrDefaultAsync<ExperimentResult?>(query, new { clientId, experimentId });

        return result;
    }

    public async Task<bool> SaveExperimentResult(int clientId, int exampleId)
    {
        await using var connection = ConnectionHelper.StartConnection(_connectionString);
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string query = "INSERT INTO clientexperimentresult (exampleid, clientid) VALUES (@exampleId, @clientId);";

            var result = await transaction.ExecuteAsync(query, new { clientId, exampleId });

            return result > 0;
        }
        catch (NpgsqlException)
        {
            await transaction.RollbackAsync();
            return false;
        }
        finally
        {
            await transaction.CommitAsync();
        }
    }
}