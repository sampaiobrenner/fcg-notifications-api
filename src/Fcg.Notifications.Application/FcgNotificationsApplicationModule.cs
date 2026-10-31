using Fcg.Notifications.Application._Shared.Behaviors;
using Fcg.Notifications.Application._Shared.Parsers;
using Fcg.Notifications.Domain._Shared.Modules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fcg.Notifications.Application;

public sealed class FcgNotificationsApplicationModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(FcgNotificationsApplicationModule).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.TryAddSingleton(TimeProvider.System);

        AddParsers(services, assembly);
    }

    private static void AddParsers(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var parsers = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.ImplementedInterfaces
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IParser<,>))
                .Select(contract => (Contract: contract, Implementation: type.AsType())));

        foreach (var (contract, implementation) in parsers)
            services.TryAddSingleton(contract, implementation);
    }
}
