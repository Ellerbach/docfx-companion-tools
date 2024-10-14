using Microsoft.Extensions.Logging;
using Moq;

namespace DocFxTocGenerator.Test.Helpers;

internal class MockLogger
{
    public static ILogger GetMockedLogger()
    {
        Mock<ILogger> logger = new Mock<ILogger>();
        return logger.Object;
    }

}
