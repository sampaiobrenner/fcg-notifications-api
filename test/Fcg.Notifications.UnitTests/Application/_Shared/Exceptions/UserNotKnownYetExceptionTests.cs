using Fcg.Notifications.Application._Shared.Exceptions;
using Fcg.Notifications.Domain._Shared.Exceptions;

namespace Fcg.Notifications.UnitTests.Application._Shared.Exceptions;

public class UserNotKnownYetExceptionTests
{
    [Fact]
    public void Excecao_NaoDeveSerBusinessException_ParaEntrarNoRetry()
    {
        var userId = Guid.CreateVersion7();

        var exception = new UserNotKnownYetException(userId);

        exception.Should().NotBeAssignableTo<BusinessException>();
        exception.UserId.Should().Be(userId);
        exception.Message.Should().Contain(userId.ToString());
    }
}
