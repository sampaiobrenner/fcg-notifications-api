using Fcg.Notifications.Application._Shared.Contexts;
using Fcg.Notifications.Application._Shared.Messaging;
using Fcg.Notifications.Domain._Shared.Modules;
using Fcg.Notifications.Infrastructure._Shared.Context;
using Fcg.Notifications.Infrastructure._Shared.Messaging;
using Fcg.Notifications.Infrastructure.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Fcg.Notifications.Infrastructure;

public sealed class FcgNotificationsInfrastructureModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(InfrastructureResources.ConnectionStringAusente);

        var npgsqlConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            GssEncryptionMode = GssEncryptionMode.Disable
        }.ConnectionString;

        services.AddDbContext<NotificationsDbContext>(options => options
            .UseNpgsql(npgsqlConnectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<INotificationsDbContext>(provider => provider.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
    }
}
