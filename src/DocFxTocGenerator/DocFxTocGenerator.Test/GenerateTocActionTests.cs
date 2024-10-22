using Bogus;
using DocFxTocGenerator.Test.Helpers;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Test;

public class GenerateTocActionTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private ILogger _logger;

    public GenerateTocActionTests()
    {
        _fileService.FillDemoSet();
        _logger = MockLogger.GetMockedLogger();
    }

    [Fact]
    public void test()
    {

    }
}
