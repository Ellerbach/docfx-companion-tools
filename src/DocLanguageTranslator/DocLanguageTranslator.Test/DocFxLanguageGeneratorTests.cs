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
    public void Translate_WithValidKey_MissingFileIsCreatedAndTextTranslated()
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
    public void Translate_WithInvalidKey_OutputFileIsNotCreated()
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

    [Fact]
    public void Translate_WithSourceLanguage_SourceLanguageIsUsed()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/de");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/de/file1.md", "# Hallo");
        mockFileService.WriteAllText("docs/en/file1.md", "# Hello");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            SourceLanguage = "en",
            Key = "key",
            Verbose = true,
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Mock translation
        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "fr"))
            .ReturnsAsync((string text, string _, string _) =>
                text.Replace("Hello", "Bonjour"));

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(0, returnValue);
        mockTranslationService.VerifyAll();
        Assert.True(mockFileService.Files.ContainsKey("docs/fr/file1.md"));
        Assert.Contains("# Bonjour", mockFileService.Files["docs/fr/file1.md"]);
    }

    [Fact]
    public void LineRange_WithoutSourceFile_ReturnsError()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            LineRange = "1-10",
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
        Assert.NotEmpty(mockMessageHelper.Errors);
    }

    [Fact]
    public void LineRange_WithInvalidFormat_ReturnsError()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.WriteAllText("docs/en/file1.md", "# Hello\nLine 2\nLine 3");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            SourceFile = "docs/en/file1.md",
            LineRange = "invalid",
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
        Assert.NotEmpty(mockMessageHelper.Errors);
    }

    [Fact]
    public void LineRange_WithNonExistentSourceFile_ReturnsError()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            SourceFile = "docs/en/nonexistent.md",
            LineRange = "1-10",
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
        Assert.NotEmpty(mockMessageHelper.Errors);
    }

    [Fact]
    public void LineRange_TranslatesSpecificLinesInExistingFiles()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Title\nLine 2 English\nLine 3 English\nLine 4");
        mockFileService.WriteAllText("docs/fr/file1.md", "# Titre\nLine 2 French OLD\nLine 3 French OLD\nLigne 4");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            SourceFile = "docs/en/file1.md",
            LineRange = "2-3",
            Verbose = true,
        };

        mockTranslationService
            .Setup(t => t.TranslateAsync(It.IsAny<string>(), "en", "fr"))
            .ReturnsAsync((string text, string _, string _) =>
                text.Replace("English", "French NEW"));

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(0, returnValue);
        string resultContent = mockFileService.Files["docs/fr/file1.md"];
        Assert.Contains("# Titre", resultContent);
        Assert.Contains("French NEW", resultContent);
        Assert.Contains("Ligne 4", resultContent);
        Assert.DoesNotContain("French OLD", resultContent);
    }

    [Fact]
    public void LineRange_WarnsWhenTargetFileDoesNotExist()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Title\nLine 2");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            Key = "valid-key",
            SourceFile = "docs/en/file1.md",
            LineRange = "1-2",
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
        Assert.Equal(0, returnValue);
        Assert.NotEmpty(mockMessageHelper.Warnings);
    }

    [Fact]
    public void LineRange_WithCheckOnly_ReportsErrorWhenTargetFileMissing()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Title\nLine 2");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            SourceFile = "docs/en/file1.md",
            LineRange = "1-2",
            CheckOnly = true,
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
        Assert.NotEmpty(mockMessageHelper.Errors);
        mockTranslationService.Verify(t => t.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void LineRange_WithCheckOnly_SucceedsWhenAllTargetFilesExist()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Title\nLine 2 to translate\nLine 3 to translate");
        mockFileService.WriteAllText("docs/fr/file1.md", "# Titre\nLigne 2\nLigne 3");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            SourceFile = "docs/en/file1.md",
            LineRange = "2-3",
            CheckOnly = true,
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
        Assert.Equal(0, returnValue);
        Assert.Empty(mockMessageHelper.Errors);
        mockTranslationService.Verify(t => t.TranslateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // Verify output shows which files would be translated
        Assert.Contains(mockMessageHelper.VerboseMessages, m => m.Contains("Would translate lines 2-3 from 'docs/en/file1.md' to 'docs/fr/file1.md'"));

        // Verify output shows the lines that would be translated
        Assert.Contains(mockMessageHelper.VerboseMessages, m => m.Contains("Line 2: Line 2 to translate"));
        Assert.Contains(mockMessageHelper.VerboseMessages, m => m.Contains("Line 3: Line 3 to translate"));
    }

    [Fact]
    public void LineRange_WithCheckOnly_DoesNotRequireKey()
    {
        // Arrange
        mockFileService.CreateDirectory("docs");
        mockFileService.CreateDirectory("docs/en");
        mockFileService.CreateDirectory("docs/fr");
        mockFileService.WriteAllText("docs/en/file1.md", "# Title\nLine 2");
        mockFileService.WriteAllText("docs/fr/file1.md", "# Titre\nLigne 2");

        CommandlineOptions options = new CommandlineOptions
        {
            DocFolder = "docs",
            SourceFile = "docs/en/file1.md",
            LineRange = "1-2",
            CheckOnly = true,
            Key = null, // No key provided
        };

        DocFxLanguageGenerator generator = new DocFxLanguageGenerator(
            options,
            mockFileService,
            mockTranslationService.Object,
            mockMessageHelper);

        // Act
        int returnValue = generator.Run();

        // Assert
        Assert.Equal(0, returnValue);
    }

    [Theory]
    [InlineData("1-10", 1, 10, true)]
    [InlineData("5-20", 5, 20, true)]
    [InlineData("100-200", 100, 200, true)]
    [InlineData("1-1", 1, 1, true)]
    [InlineData("", 0, 0, false)]
    [InlineData("invalid", 0, 0, false)]
    [InlineData("1", 0, 0, false)]
    [InlineData("1-", 0, 0, false)]
    [InlineData("-10", 0, 0, false)]
    [InlineData("10-5", 0, 0, false)]
    [InlineData("0-10", 0, 0, false)]
    [InlineData("-1-10", 0, 0, false)]
    public void TryParseLineRange_ParsesCorrectly(string input, int expectedStart, int expectedEnd, bool expectedResult)
    {
        // Arrange
        var generator = new DocFxLanguageGenerator(
            new CommandlineOptions(),
            mockFileService,
            Mock.Of<ITranslationService>(),
            mockMessageHelper);

        // Act
        bool result = generator.TryParseLineRange(input, out int startLine, out int endLine);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(expectedStart, startLine);
            Assert.Equal(expectedEnd, endLine);
        }
    }
}
