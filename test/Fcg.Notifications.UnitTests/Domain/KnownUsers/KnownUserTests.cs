using Fcg.Notifications.Domain._Shared.Exceptions;
using Fcg.Notifications.Domain.KnownUsers;

namespace Fcg.Notifications.UnitTests.Domain.KnownUsers;

public class KnownUserTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_DeveUsarUserIdComoChave_QuandoDadosValidos()
    {
        var userId = Guid.CreateVersion7();

        var user = KnownUser.Create(userId, " Ada Lovelace ", "ada@fcg.dev", Now);

        user.Id().Should().Be(userId);
        user.Name.Should().Be("Ada Lovelace");
        user.Email.Should().Be("ada@fcg.dev");
        user.PersistenceModel.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Update_DeveAtualizarNomeEmailEData_QuandoDadosValidos()
    {
        var user = KnownUser.Create(Guid.CreateVersion7(), "Ada", "ada@fcg.dev", Now);

        user.Update("Ada King", "ada.king@fcg.dev", Now.AddDays(1));

        user.Name.Should().Be("Ada King");
        user.Email.Should().Be("ada.king@fcg.dev");
        user.PersistenceModel.UpdatedAt.Should().Be(Now.AddDays(1));
    }

    [Fact]
    public void Create_DeveLancarBusinessException_QuandoUserIdVazio()
    {
        var act = () => KnownUser.Create(Guid.Empty, "Ada", "ada@fcg.dev", Now);

        act.Should().Throw<BusinessException>();
    }

    [Theory]
    [InlineData("", "ada@fcg.dev")]
    [InlineData("Ada", " ")]
    public void Update_DeveLancarBusinessException_QuandoCampoObrigatorioVazio(string name, string email)
    {
        var user = KnownUser.Create(Guid.CreateVersion7(), "Ada", "ada@fcg.dev", Now);

        var act = () => user.Update(name, email, Now);

        act.Should().Throw<BusinessException>();
    }
}
