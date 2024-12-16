// <copyright file="FileServiceTests.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text.RegularExpressions;
using Bogus;
using DocAssembler.Test.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace DocAssembler.Test;

public class FileServiceTests
{
    private Faker _faker = new();
    private MockFileService _fileService = new();
    private MockLogger _mockLogger = new();
    private ILogger _logger;

    private string _workingFolder = string.Empty;
    private string _outputFolder = string.Empty;

    public FileServiceTests()
    {
        _fileService.FillDemoSet();
        _logger = _mockLogger.Logger;

        _workingFolder = _fileService.Root;
        _outputFolder = Path.Combine(_fileService.Root, "out");
    }

    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/docs/README.md", "**/docs/**", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/src/README.md", "**/docs/**", false)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/docs/README.md", "**/*.md", true)]
    [InlineData("/Git/Projec/sharedt", "/Git/Project/shared/dotnet/MyLibrary/docs/images/machine.jpg", "**/*.md", false)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/docs/README.md", "**", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/README.md", "**", false)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/src/MyProject.Test.csproj", "**/*.Test.*", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/src/MyProject.Test.csproj", "**/*Test*", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/README.md", "*.md", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/toc.yml", "*.md", false)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/docs/README.md", "*", false)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/README.md", "*", true)]
    [InlineData("/Git/Project/shared", "/Git/Project/shared/dotnet/MyLibrary/src/MyProject.Tests.csproj", @"**/*\.Test\.*", false)]
    [InlineData("/Git/Project/backend", "/Git/Project/backend/docs/README.md", "**/docs/**", true)]
    [Theory]
    public void GlobToRegex(string root, string input, string pattern, bool selected)
    {
        // test of the Glob to Regex method in MockFileService class
        // to make sure we're having the right pattern to match files for the tests.
        string regex = _fileService.GlobToRegex(root, pattern);
        var ret = Regex.Match(input, regex).Success;
        ret.Should().Be(selected);
    }
}

