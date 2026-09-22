using Fcg.Notifications.Application;
using Fcg.Notifications.Domain;
using Fcg.Notifications.Domain._Shared.Modules;
using Fcg.Notifications.Infrastructure;
using Fcg.Notifications.WebApi;
using Fcg.Notifications.WebApi._Shared.Database;
using Fcg.Notifications.WebApi._Shared.Endpoints;
using Fcg.Notifications.WebApi._Shared.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddModule<FcgNotificationsDomainModule>(builder.Configuration)
    .AddModule<FcgNotificationsApplicationModule>(builder.Configuration)
    .AddModule<FcgNotificationsInfrastructureModule>(builder.Configuration)
    .AddModule<FcgNotificationsWebApiModule>(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync(app.Lifetime.ApplicationStopping);

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthEndpoints();
app.MapEndpoints();

await app.RunAsync();

public partial class Program;
