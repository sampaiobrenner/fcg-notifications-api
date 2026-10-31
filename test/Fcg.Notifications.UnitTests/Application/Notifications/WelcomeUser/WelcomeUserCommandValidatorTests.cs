using Fcg.Notifications.Application.Notifications.WelcomeUser;

namespace Fcg.Notifications.UnitTests.Application.Notifications.WelcomeUser;

public class WelcomeUserCommandValidatorTests
{
    private readonly WelcomeUserCommandValidator _validator = new();

    [Fact]
    public void Validate_DeveSerValido_QuandoComandoCompleto()
    {
        var result = _validator.Validate(new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "Ada", "ada@fcg.dev"));

        result.IsValid.Should().BeTrue();
    }

    public static TheoryData<WelcomeUserCommand, string> ComandosInvalidos => new()
    {
        { new WelcomeUserCommand(Guid.Empty, Guid.CreateVersion7(), "Ada", "ada@fcg.dev"), nameof(WelcomeUserCommand.EventId) },
        { new WelcomeUserCommand(Guid.CreateVersion7(), Guid.Empty, "Ada", "ada@fcg.dev"), nameof(WelcomeUserCommand.UserId) },
        { new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "", "ada@fcg.dev"), nameof(WelcomeUserCommand.Name) },
        { new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), new string('a', 121), "ada@fcg.dev"), nameof(WelcomeUserCommand.Name) },
        { new WelcomeUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), "Ada", "nao-e-email"), nameof(WelcomeUserCommand.Email) }
    };

    [Theory]
    [MemberData(nameof(ComandosInvalidos))]
    public void Validate_DeveApontarCampo_QuandoComandoInvalido(WelcomeUserCommand command, string property)
    {
        var result = _validator.Validate(command);

        result.Errors.Should().Contain(error => error.PropertyName == property);
    }
}
