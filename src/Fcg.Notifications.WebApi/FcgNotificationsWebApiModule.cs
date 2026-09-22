using Fcg.Notifications.Domain._Shared.Modules;
using Fcg.Notifications.Infrastructure._Shared.Context;
using Fcg.Notifications.WebApi._Shared.Endpoints;
using Fcg.Notifications.WebApi._Shared.Errors;
using Fcg.Notifications.WebApi._Shared.HealthChecks;
using Fcg.Notifications.WebApi._Shared.Messaging;
using Fcg.Notifications.WebApi._Shared.Security;

namespace Fcg.Notifications.WebApi;

public sealed class FcgNotificationsWebApiModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();

        services.AddJwtAuthentication(configuration);
        services.AddMessaging(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<NotificationsDbContext>("postgres", tags: [HealthCheckTags.Ready]);

        services.AddEndpoints(typeof(FcgNotificationsWebApiModule).Assembly);
    }
}
