using SAPFIAI.Application.Common.Behaviours;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Users.Commands.Login;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SAPFIAI.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<LoginCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<LoginCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<LoginCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new LoginCommand { Email = "test@test.com", Password = "password" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<LoginCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new LoginCommand { Email = "test@test.com", Password = "password" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
