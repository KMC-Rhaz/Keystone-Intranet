using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Infrastructure.InMemory;
using Keystone.Intranet.Infrastructure.Seeds;
using Microsoft.Extensions.DependencyInjection;

namespace Keystone.Intranet.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    // in-memory stores keep the app runnable before mongodb is connected; data resets on restart
    public static IServiceCollection AddKeystoneInMemoryStorage(this IServiceCollection services)
    {
        services.AddSingleton<ICompanyRepository, InMemoryCompanyRepository>();
        services.AddSingleton<IRoleRepository, InMemoryRoleRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IWorkflowDefinitionRepository, InMemoryWorkflowDefinitionRepository>();
        services.AddSingleton<IRequestRepository, InMemoryRequestRepository>();
        services.AddSingleton<IRequestHistoryRepository, InMemoryRequestHistoryRepository>();
        services.AddSingleton<ICounterRepository, InMemoryCounterRepository>();
        services.AddSingleton<IAttachmentStore, InMemoryAttachmentStore>();
        services.AddTransient<SeedRunner>();
        return services;
    }
}
