using Dapper;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Npgsql;

namespace IdCardApi.HealthChecks;

public class PlanDbHealthCheck(IConfiguration config, ILogger<PlanDbHealthCheck> logger)
    : IHealthCheck
{
    private readonly string _planDbConnexString = config.GetPlanDbConnectionString();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await using SqlConnection connex = new(_planDbConnexString);
            _ = (await connex.QueryAsync<int>(new CommandDefinition(
                    "select top 1 ck from [plan]", cancellationToken: cancellationToken)))
                .Single();
            logger.LogInformation("Plan DB healthcheck passed");
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException exc)
        {
            return HealthCheckResult.Degraded("Operation was cancelled", exc);
        }
    }
}
