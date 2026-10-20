using Fcg.Notifications.Application._Shared.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Fcg.Notifications.UnitTests.Application._Shared.Behaviors;

public class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Value);

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator()
        {
            RuleFor(request => request.Value).NotEmpty().WithMessage("valor obrigatório");
        }
    }

    [Fact]
    public async Task Handle_DeveExecutarProximoHandler_QuandoNaoHaValidadores()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new SampleRequest("válido"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeveExecutarProximoHandler_QuandoRequisicaoValida()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new SampleRequest("válido"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeveLancarValidationExceptionESemExecutarProximoHandler_QuandoRequisicaoInvalida()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);
        var nextCalled = false;

        var act = () => behavior.Handle(
            new SampleRequest(string.Empty),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();

        exception.Which.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("valor obrigatório");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeveAgregarFalhasDeTodosValidadores()
    {
        var first = new InlineValidator<SampleRequest>();
        first.RuleFor(request => request.Value).NotEmpty().WithMessage("regra 1");
        var second = new InlineValidator<SampleRequest>();
        second.RuleFor(request => request.Value).Must(_ => false).WithMessage("regra 2");
        var behavior = new ValidationBehavior<SampleRequest, string>([first, second]);

        var act = () => behavior.Handle(new SampleRequest(string.Empty), _ => Task.FromResult("ok"), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Select(error => error.ErrorMessage).Should().Equal("regra 1", "regra 2");
    }
}
