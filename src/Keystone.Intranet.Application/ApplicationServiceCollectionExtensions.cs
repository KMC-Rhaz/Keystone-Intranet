using Keystone.Intranet.Application.Dashboard;
using Keystone.Intranet.Application.Files;
using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Application.Requests;
using Keystone.Intranet.Application.Security;
using Keystone.Intranet.Application.Users;
using Keystone.Intranet.Application.Workflow;
using Keystone.Intranet.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keystone.Intranet.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddKeystoneApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BootstrapOptions>(configuration.GetSection(BootstrapOptions.SectionName));
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<FieldValueParser>();
        services.AddSingleton<ValueFormatter>();
        services.AddSingleton<PlaceholderResolver>();
        services.AddSingleton<DefinitionValidator>();
        services.AddSingleton<FileSignatureChecker>();
        services.AddSingleton<TransitionGuard>();

        services.AddScoped<FormValidator>();
        services.AddScoped<RequestWriter>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<RequestQueryService>();
        services.AddScoped<UploadService>();
        services.AddScoped<PermissionEvaluator>();
        services.AddScoped<UserSignInService>();
        services.AddScoped<UserApprovalService>();
        services.AddScoped<DashboardService>();
        return services;
    }
}
