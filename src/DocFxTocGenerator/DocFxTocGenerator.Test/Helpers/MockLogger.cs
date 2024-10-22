using Microsoft.Extensions.Logging;
using Moq;

namespace DocFxTocGenerator.Test.Helpers;

internal class MockLogger
{
    private readonly Mock<ILogger> _logger = new();

    public Mock<ILogger> Mock => _logger;
    public ILogger Logger => _logger.Object;

    public Mock<ILogger> VerifyWarningWasCalled()
    {
        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }

    public Mock<ILogger> VerifyWarningWasCalled(string expectedMessage)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }

    public Mock<ILogger> VerifyErrorWasCalled()
    {
        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }

    public Mock<ILogger> VerifyErrorWasCalled(string expectedMessage)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }

    public Mock<ILogger> VerifyCriticalWasCalled()
    {
        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }

    public Mock<ILogger> VerifyCriticalWasCalled(string expectedMessage)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        Mock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return Mock;
    }
}
