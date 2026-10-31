using Fcg.Notifications.Application._Shared.Email;
using Fcg.Notifications.Infrastructure._Shared.Context;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Fcg.Notifications.IntegrationTests._Fixtures;

/// <summary>
/// Sobe um PostgreSQL real (Testcontainers), aplica as migrations no startup da API e troca o RabbitMQ pelo
/// <see cref="ITestHarness"/> do MassTransit (transporte em memoria, mantendo consumers, definitions, retry e outbox/inbox).
/// </summary>
public sealed class NotificationsApiFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("fcg_notifications")
        .WithUsername("fcg")
        .WithPassword("fcg")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public RecordingEmailSender EmailSender { get; } = new();

    public WebApplicationFactory<Program> Factory => _factory ?? throw new InvalidOperationException("Fixture nao inicializada.");

    public ITestHarness Harness => Factory.Services.GetTestHarness();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new PostgresWebApplicationFactory(_postgres.GetConnectionString(), EmailSender);
        _ = _factory.Server;

        await Harness.Start();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        await _postgres.DisposeAsync();
    }

    public async Task<T> QueryAsync<T>(Func<NotificationsDbContext, Task<T>> query)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        return await query(context);
    }

    /// <summary>Publica no bus de teste com um <c>MessageId</c> conhecido (cada publicacao e uma nova mensagem).</summary>
    public async Task<Guid> PublishAsync<T>(T message) where T : class
    {
        var messageId = NewId.NextGuid();
        await Harness.Bus.Publish(message, context => context.MessageId = messageId);
        return messageId;
    }

    /// <summary>Envia direto para a fila informada, provando que o consumer escuta exatamente esse nome.</summary>
    public async Task<Guid> SendToQueueAsync<T>(string queueName, T message) where T : class
    {
        var messageId = NewId.NextGuid();
        var endpoint = await Harness.Bus.GetSendEndpoint(new Uri($"queue:{queueName}"));
        await endpoint.Send(message, context => context.MessageId = messageId);
        return messageId;
    }

    /// <summary>Aguarda o inbox do MassTransit registrar a mensagem como consumida com sucesso.</summary>
    public Task WaitUntilConsumedAsync(Guid messageId, TimeSpan? timeout = null)
        => WaitUntilAsync(
            () => QueryAsync(context => context.Set<InboxState>().AnyAsync(state => state.MessageId == messageId && state.Consumed != null)),
            timeout);

    public static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan? timeout = null)
    {
        using var cancellation = new CancellationTokenSource(timeout ?? DefaultTimeout);

        while (!await condition())
        {
            if (cancellation.IsCancellationRequested)
                throw new TimeoutException("A condicao esperada nao foi atingida dentro do tempo limite.");

            await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken.None);
        }
    }

    private sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly RecordingEmailSender _emailSender;

        public PostgresWebApplicationFactory(string connectionString, RecordingEmailSender emailSender)
        {
            _connectionString = connectionString;
            _emailSender = emailSender;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", _connectionString);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.UseSetting("Jwt:Key", new string('k', 32));

            builder.ConfigureTestServices(services =>
            {
                services.AddMassTransitTestHarness();
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(_emailSender);
            });
        }
    }
}

[CollectionDefinition(Name)]
public sealed class NotificationsApiCollection : ICollectionFixture<NotificationsApiFixture>
{
    public const string Name = "notifications-api";
}
