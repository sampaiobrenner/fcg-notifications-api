using System.Net;
using Fcg.Notifications.IntegrationTests._Fixtures;

namespace Fcg.Notifications.IntegrationTests.WebApi._Shared.HealthChecks;

[Collection(NotificationsApiCollection.Name)]
public class HealthReadyTests
{
    private readonly NotificationsApiFixture _fixture;

    public HealthReadyTests(NotificationsApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Ready_DeveRetornarOk_QuandoPostgresEBusDisponiveis()
    {
        using var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
