using AbTests.Api.DO;

namespace AbTests.Api.Accessors.Interfaces;

public interface ISqlAccessor
{
    Task<Client?> AddClient(Guid deviceToken);

    Task<Client?> GetClient(Guid deviceToken);

    Task<List<Experiment>> GetExperiments();

    Task<List<ExperimentExample>> GetExperimentValues(int experimentId);

    Task<ExperimentResult?> GetClientExperiment(int clientId, int experimentId);

    Task<bool> SaveExperimentResult(int clientId, int exampleId);
}