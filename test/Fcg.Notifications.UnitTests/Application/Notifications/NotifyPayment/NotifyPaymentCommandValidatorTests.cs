using Fcg.Contracts.Events.V1;
using Fcg.Notifications.Application.Notifications.NotifyPayment;

namespace Fcg.Notifications.UnitTests.Application.Notifications.NotifyPayment;

public class NotifyPaymentCommandValidatorTests
{
    private readonly NotifyPaymentCommandValidator _validator = new();

    private static NotifyPaymentCommand Valid()
        => new(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7(), 199.9m, PaymentStatus.Approved, null);

    [Fact]
    public void Validate_DeveSerValido_QuandoComandoCompleto()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    public static TheoryData<NotifyPaymentCommand, string> ComandosInvalidos => new()
    {
        { Valid() with { EventId = Guid.Empty }, nameof(NotifyPaymentCommand.EventId) },
        { Valid() with { OrderId = Guid.Empty }, nameof(NotifyPaymentCommand.OrderId) },
        { Valid() with { UserId = Guid.Empty }, nameof(NotifyPaymentCommand.UserId) },
        { Valid() with { Price = -1m }, nameof(NotifyPaymentCommand.Price) },
        { Valid() with { Status = (PaymentStatus)99 }, nameof(NotifyPaymentCommand.Status) }
    };

    [Theory]
    [MemberData(nameof(ComandosInvalidos))]
    public void Validate_DeveApontarCampo_QuandoComandoInvalido(NotifyPaymentCommand command, string property)
    {
        _validator.Validate(command).Errors.Should().Contain(error => error.PropertyName == property);
    }
}
