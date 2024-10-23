namespace DocLinkChecker.Test
{
    using System.Collections.Generic;
    using DocLinkChecker.Interfaces;
    using DocLinkChecker.Test.Helpers;

    public class MockFileService : IFileService
    {
        public string Root = "d:\\Git\\Project\\docs";

        public Dictionary<string, string> Files { get; set; } = new();

        public void FillDemoSet()
        {
            Files.Clear();

            Files.Add($"{Root}\\index.md", string.Empty
                .AddHeading("Main index", 1)
                .AddParagraphs(3));

            Files.Add($"{Root}\\getting-started", null);

            Files.Add($"{Root}\\getting-started\\README.md", string.Empty
                .AddHeading("Getting started", 1)
                .AddParagraphs(1).AddLink("../general")
                .AddParagraphs(1).AddLink("../general/general-sample.md"));

            Files.Add($"{Root}\\general", null);

            Files.Add($"{Root}\\general\\README.md", string.Empty
                .AddHeading("General documentation", 1)
                .AddParagraphs(1).AddLink("./general-sample.md")
                .AddParagraphs(1).AddLink("./another-sample.md"));

            Files.Add($"{Root}\\general\\general-sample.md", string.Empty
                .AddHeading("Sample General Document", 1)
                .AddParagraphs(1).AddLink("https://loremipsum.io/generator/?n=5&t=p")
                .AddParagraphs(1).AddLink("./another-sample.md#third1-header")
                .AddParagraphs(1).AddLink("http://ww1.microchip.com/downloads/en/devicedoc/21295c.pdf")
                .AddNewLine()
                .AddResourceLink("./images/nature.jpeg")
                .AddParagraphs(1).AddLink("./another-sample.md")
                .AddNewLine()
                .AddResourceLink("images/another-image.png")
                .AddParagraphs(1).AddLink("https://www.hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx")
                .AddNewLine()
                .AddTableStart(3)
                .AddTableRow("Microsoft", "<https://www.microsoft.com>", string.Empty.AddLink("https://blogs.microsoft.com/"))
                .AddTableRow(".NET Foundation", "<https://dotnetfoundation.org/>", string.Empty.AddLink("https://dotnetfoundation.org/about/faq"))
                .AddTableRow("Github", "<https://github.com/> ", string.Empty.AddLink("https://github.blog/"))
                .AddNewLine()
                .AddParagraphs(2));

            Files.Add($"{Root}\\general\\another-sample.md", string.Empty
                .AddHeading("Another Sample Document", 1)
                .AddParagraphs(1)
                .AddHeading("First header", 2).AddParagraphs(1)
                .AddHeading("Second header", 2).AddParagraphs(1)
                .AddHeading("Third header", 2).AddParagraphs(1)
                .AddHeading("Third.1 header", 2).AddParagraphs(1)
                .AddHeading("Fourth header", 2).AddParagraphs(1));

            Files.Add($"{Root}\\general\\images\\nature.jpeg", "<image>");
            Files.Add($"{Root}\\general\\images\\another-image.png", "<image>");
            Files.Add($"{Root}\\general\\images\\space image.jpeg", "<image>");

            Files.Add($"{Root}\\src", null);
            Files.Add($"{Root}\\src\\sample.cs", @"namespace MySampleApp;

public class SampleClass
{
    public void SampleMethod()
    {
        // <MainLoop>
        foreach(var thing in list)
        {
             // Do Stuff
        }
        // </MainLoop>
    }
}");
        }

        public void DeleteFile(string path)
        {
        }

        public void DeleteFiles(string[] paths)
        {
        }

        public bool ExistsFileOrDirectory(string path)
        {
            return Files.ContainsKey(path.Replace("/", "\\"));
        }

        public string GetDirectory(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }

        public IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes)
        {
            List<string> files = new();
            foreach (string key in Files.Keys)
            {
                // only get files (with content), otherwise it's a directory
                if (Files.TryGetValue(key, out string content) && !string.IsNullOrEmpty(content))
                {
                    files.Add(key);
                }
            }
            return files;
        }

        public string GetFullPath(string path)
        {
            return path;
        }

        public string GetRelativePath(string relativeTo, string path)
        {
            return path;
        }
    }
}
