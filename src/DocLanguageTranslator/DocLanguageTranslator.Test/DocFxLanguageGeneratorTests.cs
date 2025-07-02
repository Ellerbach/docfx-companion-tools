// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using DocFXLanguageGenerator.Helpers;

namespace DocLanguageTranslator.Test;

public sealed class DocFxLanguageGeneratorTests
{
    private readonly MockFileService mockFileService;
    private readonly Mock<ITranslationService> mockTranslationService;
    private readonly MockMessageHelper mockMessageHelper;

    public DocFxLanguageGeneratorTests()
    {
        mockFileService = new MockFileService();
        mockTranslationService = new Mock<ITranslationService>();
        mockMessageHelper = new MockMessageHelper();
    }

    [Fact]
    public void Run_WithoutSubscriptionKey_ReturnsError()
    {
        CommandlineOptions options = new CommandlineOptions
        {
            CheckOnly = false,
            Verbose = true,
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(1, returnValue);
        Assert.Collection(
            mockMessageHelper.Errors,
            x =>
            {
                Assert.Equal("ERROR: you have to have an Azure Cognitive Service key if you are not only checking the structure.", x);
            });
    }

    [Fact]
    public void Run_WithoutExistingDocsFolder_ReturnsError()
    {
        CommandlineOptions options = new CommandlineOptions
        {
            CheckOnly = true,
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(1, returnValue);
        Assert.Collection(
            mockMessageHelper.Errors,
            x =>
            {
                Assert.Equal("ERROR: Documentation folder '' doesn't exist.", x);
            });
    }

    [Fact]
    public void Translate_SupportsComplexLanguageCodes()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/zh-Hans");
        mockFileService.CreateDirectory("docs/zh-Hant");
        mockFileService.WriteAllText("docs/en/file1.md", "# Hello");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            Verbose = true,
        };

        // Mock different translations for different Chinese variants
        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "zh-Hans"))
            .ReturnsAsync("# 你好 (简体)");

        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "zh-Hant"))
            .ReturnsAsync("# 你好 (繁體)");

        var generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(0, returnValue);
        Assert.Contains("# 你好 (简体)", mockFileService.Files["docs/zh-Hans/file1.md"]);
        Assert.Contains("# 你好 (繁體)", mockFileService.Files["docs/zh-Hant/file1.md"]);
    }

    [Fact]
    public void GetLanguageCodeFromPath_HandlesAllFormats()
    {
        // Arrange
        var generator = new DocFxLanguageGenerator(
            new CommandlineOptions(),
            mockFileService,
            Mock.Of<ITranslationService>(),
            mockMessageHelper);

        // Act & Assert
        Assert.Equal("en", generator.GetLanguageCodeFromPath("/path/to/en"));
        Assert.Equal("fr", generator.GetLanguageCodeFromPath("C:\\docs\\fr"));
        Assert.Equal("zh-Hans", generator.GetLanguageCodeFromPath("/docs/zh-Hans"));
        Assert.Equal("zh-Hant", generator.GetLanguageCodeFromPath("C:\\docs\\zh-Hant"));
        Assert.Equal("pt-br", generator.GetLanguageCodeFromPath("/docs/pt-br")); // Case preserved
        Assert.Equal("en", generator.GetLanguageCodeFromPath("en")); // Single directory
    }

    [Fact]
    public async Task Translate_WithValidKey_MissingFileIsCreatedAndTextTranslated()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Hello");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            Verbose = true,
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Mock successful translation
        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "fr"))
            .ReturnsAsync((string text, string _, string _) =>
                text.Replace("Hello", "Bonjour"));

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(0, returnValue);
        Assert.True(mockFileService.Files.ContainsKey("docs/fr/file1.md"));
        Assert.Contains("# Bonjour", mockFileService.Files["docs/fr/file1.md"]);
    }

    [Fact]
    public async Task Translate_WithInvalidKey_OutputFileIsNotCreated()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Hello");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "invalid",
            Verbose = true,
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Mock failed translation
        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "fr"))
            .ThrowsAsync(new TranslationException("The request is not authorized because credentials are missing or invalid.", 401000));

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(1, returnValue);
        Assert.False(mockFileService.Files.ContainsKey("docs/fr/file1.md"));
    }
}
