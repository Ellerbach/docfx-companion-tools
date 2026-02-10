// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using DocLanguageTranslator.FileService;
using System.IO;
using System.Linq;

namespace DocLanguageTranslator.Test.Helpers;

public class MockFileService : IFileService
{
    public Dictionary<string, string> Files { get; } = new Dictionary<string, string>();
    public List<string> Directories { get; } = new List<string>();

    public bool DirectoryExists(string path) => Directories.Contains(path);

    public bool FileExists(string path) => Files.ContainsKey(path);

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        => Files.Keys
            .Where(k => k.StartsWith(path) && k.EndsWith(".md"))
            .ToArray();

    public string[] GetDirectories(string path)
        => Directories.Where(d => d.StartsWith(path)).ToArray();

    public string ReadAllText(string filePath)
        => Files.TryGetValue(filePath, out var content) ? content : null;

    public void WriteAllText(string filePath, string content)
        => Files[filePath] = content;

    public void CreateDirectory(string path)
    {
        if (!Directories.Contains(path))
            Directories.Add(path);
    }

    public string[] ReadAllLines(string filePath)
    {
        if (Files.TryGetValue(filePath, out var content))
        {
            return content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        return [];
    }

    public string[] ReadLines(string filePath, int startLine, int endLine)
    {
        var allLines = ReadAllLines(filePath);
        int start = Math.Max(0, startLine - 1);
        int end = Math.Min(allLines.Length, endLine);
        int count = end - start;

        if (count <= 0)
        {
            return [];
        }

        return allLines.Skip(start).Take(count).ToArray();
    }

    public void ReplaceLines(string filePath, int startLine, int endLine, string[] newLines)
    {
        var allLines = ReadAllLines(filePath).ToList();
        int start = Math.Max(0, startLine - 1);
        int end = Math.Min(allLines.Count, endLine);
        int count = end - start;

        if (count > 0)
        {
            allLines.RemoveRange(start, count);
        }

        allLines.InsertRange(start, newLines);
        WriteAllText(filePath, string.Join("\n", allLines));
    }
}
