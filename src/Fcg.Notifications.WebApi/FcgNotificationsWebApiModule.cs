using System.Text.Json.Serialization;
using Fcg.Notifications.Application._Shared.Security;
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
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddJwtAuthentication(configuration);
        services.AddMessaging(configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<NotificationsDbContext>("postgres", tags: [HealthCheckTags.Ready]);

        services.AddEndpoints(typeof(FcgNotificationsWebApiModule).Assembly);
    }
}
