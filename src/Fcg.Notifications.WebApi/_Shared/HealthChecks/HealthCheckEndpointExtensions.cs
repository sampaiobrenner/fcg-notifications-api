using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Fcg.Notifications.WebApi._Shared.HealthChecks;

public static class HealthCheckEndpointExtensions
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckTags.Ready) })
            .AllowAnonymous();

        return app;
    }
}
