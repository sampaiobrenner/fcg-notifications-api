using System.Text.Json;
using System.Text.Json.Serialization;
using Fcg.Notifications.Domain._Shared.Exceptions;
using Fcg.Notifications.Infrastructure._Shared.Context;
using FluentValidation;
using MassTransit;
using MassTransit.Serialization;

namespace Fcg.Notifications.WebApi._Shared.Messaging;

public static class MessagingServiceCollectionExtensions
{
    private static readonly TimeSpan[] RetryIntervals =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(30)
    ];

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        // Payload conforme o contrato (fcg-contracts/samples): enums como string ("status": "Approved") e decimais como
        // numero ("price": 199.90). O padrao do MassTransit escreve enum como numero e decimal como texto. As opcoes do
        // serializer sao globais; configuradas aqui valem para qualquer transporte (RabbitMQ ou harness em memoria).
        SystemTextJsonMessageSerializer.Options = WithContractConventions(SystemTextJsonMessageSerializer.Options);

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumers(typeof(MessagingServiceCollectionExtensions).Assembly);

            bus.AddEntityFrameworkOutbox<NotificationsDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            // Retry e outbox/inbox por endpoint (nesta ordem): cada tentativa usa um escopo/DbContext novo, e a politica
            // vale para qualquer transporte (RabbitMQ em execucao, in-memory no ITestHarness dos testes de integracao).
            bus.AddConfigureEndpointsCallback((context, _, endpoint) =>
            {
                endpoint.UseMessageRetry(retry =>
                {
                    retry.Intervals(RetryIntervals);
                    retry.Ignore<BusinessException>();
                    retry.Ignore<ValidationException>();
                });

                endpoint.UseEntityFrameworkOutbox<NotificationsDbContext>(context);
            });

            bus.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Host(options.Host, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);
                });

                rabbit.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static JsonSerializerOptions WithContractConventions(JsonSerializerOptions options)
    {
        if (options.Converters.Any(converter => converter is DecimalNumberJsonConverter))
            return options;

        var configured = new JsonSerializerOptions(options);

        foreach (var decimalConverter in configured.Converters.Where(converter => converter is JsonConverter<decimal>).ToList())
            configured.Converters.Remove(decimalConverter);

        configured.Converters.Insert(0, new DecimalNumberJsonConverter());
        configured.Converters.Add(new JsonStringEnumConverter());
        return configured;
    }
}
