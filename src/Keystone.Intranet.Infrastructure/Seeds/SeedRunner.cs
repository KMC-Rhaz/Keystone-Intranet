using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Keystone.Intranet.Infrastructure.Seeds;

// inserts only what is missing so admin edits survive restarts
public sealed class SeedRunner(
    ICompanyRepository companies,
    IRoleRepository roles,
    IWorkflowDefinitionRepository definitions,
    TimeProvider clock,
    ILogger<SeedRunner> logger)
{
    public static IReadOnlyList<WorkflowDefinition> Definitions() =>
    [
        CashAdvanceDefinition.Create(),
        LeaveRequestDefinition.Create(),
        PaymentCollectionDefinition.Create(),
        ReceiptSubmissionDefinition.Create(),
    ];

    public async Task RunAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var added = 0;

        foreach (var company in CompanySeeds.All())
        {
            company.CreatedAt = company.UpdatedAt = now;
            if (await companies.InsertIfMissingAsync(company, ct)) added++;
        }

        foreach (var role in RoleSeeds.All())
        {
            role.CreatedAt = role.UpdatedAt = now;
            if (await roles.InsertIfMissingAsync(role, ct)) added++;
        }

        foreach (var definition in Definitions())
        {
            definition.CreatedAt = definition.UpdatedAt = now;
            if (await definitions.InsertIfMissingAsync(definition, ct)) added++;
        }

        logger.LogInformation("seed finished, {Added} documents added", added);
    }
}
