// <copyright file="MockFileService.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using System.Text;
using System.Text.RegularExpressions;
using DocAssembler.FileService;

namespace DocAssembler.Test.Helpers;

public class MockFileService : IFileService
{
    public string Root;

    public Dictionary<string, string> Files { get; set; } = new();

    public MockFileService()
    {
        // determine if we're testing on Windows. If not, use linux paths.
        if (Path.IsPathRooted("c://"))
        {
            // windows
            Root = "c:/Git/Project";
        }
        else
        {
            // linux
            Root = "/Git/Project";
        }
    }

    public void FillDemoSet()
    {
        Files.Clear();

        // make sure that root folders are available
        EnsurePath(Root);
        
        // 4 files
        var folder = AddFolder(".docfx");
        AddFile(folder, "docfx.json", "<docfx configuration>");
        AddFile(folder, "index.md", string.Empty
            .AddHeading("Test Repo", 1)
            .AddParagraphs(1)
            .AddRawLink("Keyboard", "images/keyboard.jpg")
            .AddParagraphs(1)
            .AddRawLink("setup your dev machine", "./general/getting-started/setup-dev-machine.md"));
        AddFile(folder, "toc.yml",
@"---
- name: General
  href: general/README.md
- name: Services
  href: services/README.md");
        folder = AddFolder(".docfx/images");
        AddFile(folder, "keyboard.jpg", "<image>");

        // docs: 1 + 2 + 1 + 3 = 7 files
        folder = AddFolder($"docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Documentation Readme", 1)
            .AddParagraphs(3));
        folder = AddFolder("docs/getting-started");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Getting Started", 1)
            .AddRaw("EXTERNAL: [.docassemble.json](../../.docassemble.json)")
            .AddRaw("WEBLINK: [Microsoft](https://www.microsoft.com)")
            .AddRaw("RESOURCE: ![computer](assets/computer.jpg)")
            .AddRaw("PARENT-DOC: [Docs readme](../README.md)")
            .AddRaw("RELATIVE-DOC: [Documentation guidelines](../guidelines/documentation-guidelines.md)")
            .AddRaw("ANOTHER-SUBFOLDER-DOC: [Documentation guidelines](../guidelines/documentation-guidelines.md)")
            .AddRaw("ANOTHER-DOCS-TREE: [System Copilot](../../tools/system-copilot/docs/README.md#usage)")
            .AddRaw("ANOTHER-DOCS-TREE-BACKSLASH: [System Copilot](..\\..\\tools\\system-copilot\\docs\\README.md#usage)"));
        folder = AddFolder("docs/getting-started/assets");
        AddFile(folder, "computer.jpg", "<picture>");
        folder = AddFolder("docs/guidelines");
        AddFile(folder, "documentation-guidelines.md", string.Empty
            .AddHeading("Documentation Guidelines", 1)
            .AddRaw("STANDARD: AB#1234 reference")
            .AddRaw("AB#4321 at START")
            .AddRaw("EMPTY-LINK: [AB#1000]() is okay."));
        AddFile(folder, "dotnet-guidelines.md", string.Empty
            .AddHeading(".NET Guidelines", 1)
            .AddParagraphs(3));

        // 
        folder = AddFolder("backend");
        folder = AddFolder("backend/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Backend", 1)
            .AddParagraphs(2));
        folder = AddFolder("backend/app1");
        folder = AddFolder("backend/app1/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("App1", 1)
            .AddRaw(@"We're using the [system copilot](../../../tools/system-copilot/docs/README.md#usage)")
            .AddParagraphs(2)
            );
        folder = AddFolder("backend/app1/src");
        AddFile(folder, "app1.cs", "<code of app1>");
        folder = AddFolder("backend/subsystem1");
        folder = AddFolder("backend/subsystem1/docs");
        AddFile(folder, "explain-subsystem.md", string.Empty
            .AddHeading("Subsystem 1", 1)
            .AddParagraphs(3));
        folder = AddFolder("backend/subsystem1/app20");
        folder = AddFolder("backend/subsystem1/app20/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("app20", 1)
            .AddRaw(
@"This is part of [the subsystem1](../../docs/explain-subsystem1.md).

We're following the [Documentation Guidelines](../../../../docs/guidelines/documentation-guidelines.md)")
            .AddParagraphs(1)
            .AddRaw("It's also important to look at [the code](../src/app20.cs)")
            .AddParagraphs(3));
        folder = AddFolder("backend/subsystem1/app20/src");
        AddFile(folder, "app20.cs", "<code of app20>");
        folder = AddFolder("backend/subsystem1/app30");
        folder = AddFolder("backend/subsystem1/app30/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("app30", 1)
            .AddRaw(
@"This is part of [the subsystem1](../../docs/explain-subsystem1.md).

We're using [My Library](../../../../shared/dotnet/MyLibrary/docs/README.md) in this app.")
            .AddParagraphs(2));
        folder = AddFolder("backend/subsystem1/app30/src");
        AddFile(folder, "app30.cs", "<code of app30>");

        folder = AddFolder("shared");
        folder = AddFolder("shared/dotnet");
        folder = AddFolder("shared/dotnet/MyLibrary");
        folder = AddFolder("shared/dotnet/MyLibrary/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("My Library", 1)
            .AddParagraphs(2));
        folder = AddFolder("shared/dotnet/MyLibrary/src");
        AddFile(folder, "MyLogic.cs", "<MyLogic source code>");

        folder = AddFolder("tools");
        folder = AddFolder("tools/system-copilot");
        folder = AddFolder("tools/system-copilot/docs");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("The system copilot", 1)
            .AddParagraphs(2)
            .AddRaw("Go to [usage][#usage]")
            .AddHeading("Usage", 2)
            .AddRaw("You can use the tool like this:")
            .AddRaw(
@"```shell
system-copilot -q ""provide your question here""
```"));
        folder = AddFolder("tools/system-copilot/SRC");
        AddFile(folder, "system-copilot.cs", "<copilot source code>");
    }

    public string AddFolder(string relativePath)
    {
        var fullPath = Path.Combine(Root, relativePath).NormalizePath();
        Files.Add(fullPath, string.Empty);
        return relativePath;
    }

    public void AddFile(string folderPath, string filename, string content)
    {
        var fullPath = Path.Combine(Root, folderPath, filename).NormalizePath();
        Files.Add(fullPath, content.NormalizeContent());
    }

    public void Delete(string path)
    {
        Files.Remove(GetFullPath(path));
    }

    public void Delete(string[] paths)
    {
        foreach (var path in paths)
        {
            Files.Remove(GetFullPath(path));
        }
    }

    public bool ExistsFileOrDirectory(string path)
    {
        string fullPath = GetFullPath(path);
        return Root.Equals(fullPath, StringComparison.OrdinalIgnoreCase) || Files.ContainsKey(fullPath);
    }

    public IEnumerable<string> GetFiles(string root, List<string> includes, List<string>? excludes)
    {
        string fullRoot = GetFullPath(root);

        List<string> rgInc = includes.Select(x => GlobToRegex(root, x)).ToList();
        List<string> rgExc = [];
        if (excludes is not null)
        {
            rgExc = excludes.Select(x => GlobToRegex(root, x)).ToList();
        }

        List<string> files = [];

        var filesNoFolders = Files.Where(x => !string.IsNullOrEmpty(x.Value));
        foreach (var file in filesNoFolders)
        {
            string selection = string.Empty;
            // see if it matches any of the include patterns
            foreach (string pattern in rgInc)
            {
                if (Regex.Match(file.Key, pattern).Success)
                {
                    // yes, so we're done here
                    selection = file.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(selection))
            {
                // see if it's excluded by any pattern
                foreach (string pattern in rgExc)
                {
                    if (Regex.Match(file.Key, pattern).Success)
                    {
                        // yes, so we can skip this one
                        selection = string.Empty;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(selection))
            {
                // still have a selection, so add it to the list.
                files.Add(selection);
            }
        }

        return files;
    }

    public string GetFullPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path.NormalizePath();
        }
        else
        {
            return Path.Combine(Root, path).NormalizePath();
        }
    }

    public string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path).NormalizePath();
    }

    public IEnumerable<string> GetDirectories(string folder)
    {
        return Files.Where(x => x.Value == string.Empty &&
                                x.Key.StartsWith(GetFullPath(folder)) &&
                                !x.Key.Substring(Math.Min(GetFullPath(folder).Length + 1, x.Key.Length)).Contains("/") &&
                                !x.Key.Equals(GetFullPath(folder), StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key).ToList();
    }

    public string ReadAllText(string path)
    {
        string ipath = GetFullPath(path);
        if (Files.TryGetValue(ipath, out var content) && !string.IsNullOrEmpty(content))
        {
            return content.NormalizeContent();
        }

        throw new FileNotFoundException($"File not found: '{path}'");
    }

    public string[] ReadAllLines(string path)
    {
        if (Files.TryGetValue(GetFullPath(path), out var content) && !string.IsNullOrEmpty(content))
        {
            return content.NormalizeContent().Split("\n");
        }

        throw new FileNotFoundException($"File not found: '{path}'");
    }

    public void WriteAllText(string path, string content)
    {
        string ipath = GetFullPath(path);
        if (Files.TryGetValue(ipath, out var x))
        {
            Files.Remove(ipath);
        }
        Files.Add(ipath, content!.NormalizeContent());
    }

    public Stream OpenRead(string path)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(ReadAllText(path)));
    }

    public void Copy(string source, string destination)
    {
        string file = Path.GetFileName(destination);
        string path = Path.GetDirectoryName(destination)!.NormalizePath();

        if (!ExistsFileOrDirectory(source))
        {
            throw new FileNotFoundException($"Source file {source} not found");
        }

        EnsurePath(path);

        if (!ExistsFileOrDirectory(destination))
        {
            string content = ReadAllText(source);
            AddFile(path, file, content);
        }
    }

    public void DeleteFolder(string path)
    {
        Delete(path);
    }

    public string GlobToRegex(string root, string input)
    {
        // replace **/*.<ext> where <ext> is the extension. E.g. **/*.md
        string pattern = @"\*\*\\\/\*\\\.(?<ext>\w+)";
        if (Regex.Match(input, pattern).Success)
        {
            return root.TrimEnd('/') + "/" + Regex.Replace(input, pattern, @".+\/*\.${ext}$");
        }

        // replace **/*<part>* where <part> can be any test. E.g. **/*.Test.*
        pattern = @"\*\*\/\*(?<part>.+)\*";
        if (Regex.Match(input, pattern).Success)
        {
            return Regex.Replace(input, pattern, ".+${part}.+$");
        }

        // replace **
        pattern = @"\*\*";
        if (Regex.Match(input, pattern).Success)
        {
            return root.TrimEnd('/') + Regex.Replace(input, pattern, ".*/*");
        }

        // replace *.<ext> where <ext> is the extension. E.g. *.md
        pattern = @"\*\.(?<ext>\w+)$";
        if (Regex.Match(input, pattern).Success)
        {
            return root.TrimEnd('/') + "/" + Regex.Replace(input, pattern, @"[^\/\\]*\.${ext}$");
        }

        // replace *
        pattern = @"\*";
        if (Regex.Match(input, pattern).Success)
        {
            return root.TrimEnd('/') + "/" + Regex.Replace(input, pattern, @"[^\/\\]*$");
        }

        return input;
    }

    private void EnsurePath(string path)
    {
        // ensure path exists
        string[] elms = path.Split('/');
        string elmPath = string.Empty;
        foreach (string elm in elms)
        {
            if (!string.IsNullOrEmpty(elm))
            {
                elmPath += "/";
            }
            elmPath += elm;
            if (!ExistsFileOrDirectory(elmPath))
            {
                AddFolder(elmPath);
            }
        }
    }
}

