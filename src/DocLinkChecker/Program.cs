// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocLinkChecker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using CommandLine;
    using DocLinkChecker.Domain;
    using DocLinkChecker.Helpers;

    /// <summary>
    /// Main program class for documentation link checker tool. It's a command-line tool
    /// that takes parameters. Use -help as parameter to see the syntax.
    /// </summary>
    public class Program
    {
        private static CommandlineOptions options;
        private static MessageHelper message;

        private static DirectoryInfo rootDir;
        private static List<string> allFiles = new List<string>();
        private static List<string> allLinks = new List<string>();

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Commandline options described in <see cref="CommandlineOptions"/> class.</param>
        /// <returns>0 if succesful, 1 on error.</returns>
        private static int Main(string[] args)
        {
            // Set default exit code
            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.OK;

            try
            {
                Parser.Default.ParseArguments<CommandlineOptions>(args)
                                   .WithParsed<CommandlineOptions>(RunLogic)
                                   .WithNotParsed(HandleErrors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Parsing arguments threw an exception with message `{ex.Message}`");
                ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
            }

            Console.WriteLine($"Exit with return code {(int)ExitCodeHelper.ExitCode}");

            return (int)ExitCodeHelper.ExitCode;
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        private static void RunLogic(CommandlineOptions o)
        {
            options = o;
            message = new MessageHelper(options);

            // correction needed if relative path is given as parameter
            o.DocFolder = Path.GetFullPath(o.DocFolder);

            message.Verbose($"Documentation folder: {options.DocFolder}");
            message.Verbose($"Verbose             : {options.Verbose}");

            foreach (string file in Directory.EnumerateFiles(options.DocFolder, "*.*", SearchOption.AllDirectories))
            {
                allFiles.Add(file.ToLowerInvariant());
            }

            if (!Directory.Exists(options.DocFolder))
            {
                message.Error($"ERROR: Documentation folder '{options.DocFolder}' doesn't exist.");
                ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                return;
            }

            ValidateDocFolder(options.DocFolder);

            // we start at the root to generate the TOC items
            rootDir = new DirectoryInfo(options.DocFolder);
            WalkDirectoryTree(rootDir);

            if (options.Attachments)
            {
                CheckUnreferencedAttachments();
            }
        }

        /// <summary>
        /// On parameter errors, we set the exit code to 1 to indicated a parsing error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<Error> errors)
        {
            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
        }

        /// <summary>
        /// Validate that the root directory is a valid DocFX root.
        /// If the directory is not valid, warn the caller.
        /// </summary>
        private static void ValidateDocFolder(string docfolder)
        {
            message.Verbose($"Validating documentation folder {docfolder}");

            const string docFXProjectFile = "docfx.json";
            List<string> rootFiles = Directory.GetFiles(docfolder).ToList().ConvertAll(f => Path.GetFileName(f).ToLowerInvariant());

            // notify client if root directory does not include docfx.json file
            if (!rootFiles.Contains(docFXProjectFile))
            {
                message.Warning("Documentation folder does not contain docfx.json file. Links relative to project root (i.e. ~/path/from/root) may not be properly identified.");
            }
        }

        /// <summary>
        /// Main function going through all the folders, files and subfolders.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        private static void WalkDirectoryTree(DirectoryInfo folder)
        {
            message.Verbose($"Processing folder {folder.FullName}");

            // process MD files in this folder
            ProcessFiles(folder);

            // process other sub folders
            DirectoryInfo[] subDirs = folder.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirs)
            {
                WalkDirectoryTree(dirInfo);
            }
        }

        /// <summary>
        /// Check the .attachments folder for files that are not referenced by any of the docs.
        /// If the cleanup option is given as well, we'll remove those files from disc.
        /// </summary>
        private static void CheckUnreferencedAttachments()
        {
            string attachmentsPath = Path.Combine(options.DocFolder, ".attachments");
            bool errorHeader = false;

            if (allLinks.Any() && Directory.Exists(attachmentsPath))
            {
                List<string> attachments = Directory.GetFiles(attachmentsPath).ToList();
                foreach (string attachment in attachments)
                {
                    if (!allLinks.Contains(attachment.ToLowerInvariant()))
                    {
                        if (options.Cleanup)
                        {
                            File.Delete(attachment);
                            message.Warning($"{attachment} deleted.");
                        }
                        else
                        {
                            if (!errorHeader)
                            {
                                message.Output($"\nFiles not referenced:\n");
                                errorHeader = true;
                            }

                            message.Error($"{attachment}");

                            // mark error in exit code of the tool
                            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the list of the files in the current directory.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        private static void ProcessFiles(DirectoryInfo folder)
        {
            message.Verbose($"Process {folder.FullName} for files.");

            List<FileInfo> files = folder.GetFiles("*.md").OrderBy(f => f.Name).ToList();
            if (files == null)
            {
                message.Verbose($"No MD files found in {folder.FullName}.");
                return;
            }

            foreach (FileInfo fi in files)
            {
                message.Verbose($"Processing {fi.FullName}");
                string content = File.ReadAllText(fi.FullName);

                // first see if there are links in this file
                Regex rxContent = new Regex(@"(\[{1}.*\]{1}\({1}.*\){1}?)");
                if (rxContent.Matches(content).Any())
                {
                    message.Verbose($"- Links detected.");

                    // it has references, so check in detail
                    ProcessFile(folder, fi.FullName);
                }

                if (options.Table)
                {
                    Regex rxTable = new Regex(@"\|(?:.*)\|");
                    if (rxTable.Matches(content).Any())
                    {
                        message.Verbose($"- Processing table.");
                        ProcessTable(fi.FullName, content.Replace("\r", string.Empty).Split('\n'));
                    }
                }

                message.Verbose($"{fi.FullName} processed.");
            }
        }

        /// <summary>
        /// Process a file to check the integrity of the table.
        /// </summary>
        /// <param name="filepath">The full file name.</param>
        /// <param name="content">The content split into lines.</param>
        private static void ProcessTable(string filepath, string[] content)
        {
            // A table is a list of |, the first line will determine how many columns
            // The second line should contain at least 3 separators '-' between the |
            // After each line, there should not be any text after the last |
            Regex rxTable = new Regex(@"\|(?:.*)\|");
            Regex rxTableFormatRow = new Regex(@"\s*(\|\s*:?-+:?\s*)+\|\s*");
            Regex rxComments = new Regex(@"\`(?:[^`]*)\`");
            int idxLine = 0;
            bool isMatch = false;
            int numCol = 0;
            int initLine = 0;
            bool isCodeBlock = false;
            char[] charsToTrim = { '\r', '\n' };
            int minimalValidLineToTestIndex = 2;
            for (int i = 0; i < content.Length; i++)
            {
                string line = content[i];
                if (line.StartsWith("```"))
                {
                    isCodeBlock = !isCodeBlock;
                }

                // Check if there is blank line before table (required by DocFX)
                if (i >= minimalValidLineToTestIndex &&
                    rxTableFormatRow.Matches(line).Any() &&
                    (rxTableFormatRow.Match(line).Value.Length == line.Length) &&
                    !isCodeBlock)
                {
                    string lineToTest = content[i - minimalValidLineToTestIndex];
                    if (lineToTest.Trim(charsToTrim) != string.Empty)
                    {
                        message.Error($"Malformed table in {filepath}, line {i - 1}. Blank line expected before table.");
                        ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                    }
                }

                if (rxTable.Matches(line).Any() && !isCodeBlock)
                {
                    isMatch = true;
                    message.Verbose($"Table found line {i}.");

                    // Check if line ends with a | when the lines starts with a |
                    if (!line.EndsWith('|') && line.Replace(" ", string.Empty).StartsWith('|'))
                    {
                        message.Error($"Malformed table in {filepath}, line {i + 1}. Table should finish by character '|'.");
                        ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                    }

                    if (line.EndsWith('|') && !line.Replace(" ", string.Empty).StartsWith('|'))
                    {
                        message.Error($"Malformed table in {filepath}, line {i + 1}. Table should start by character '|'.");
                        ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                    }

                    // Is it first line?
                    if (idxLine == 0)
                    {
                        initLine = i;

                        // How many columns
                        numCol = line.Count(m => m == '|') - 1;
                        message.Verbose($"Number of columns: {numCol}");
                    }
                    else
                    {
                        if (i != initLine + idxLine)
                        {
                            message.Error($"Malformed table in {filepath}, line {i + 1}. Table should be continuous.");
                            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                        }

                        // Remove comments inside allowing something like `Spike \| data` to not count as a |
                        if (rxComments.Matches(line).Any())
                        {
                            line = rxComments.Replace(line, string.Empty);
                        }

                        // Count separators
                        string[] separators = line.Split('|');
                        if (separators.Length - 2 != numCol)
                        {
                            message.Error($"Malformed table in {filepath}, line {i + 1}. Different number of columns {separators.Length - 2} vs {numCol}.");
                            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                        }

                        if (idxLine == 1)
                        {
                            for (int sep = 1; sep < separators.Length - 1; sep++)
                            {
                                if (separators[sep].Count(m => m == '-') < 3)
                                {
                                    message.Error($"Malformed table in {filepath}, line {i + 1}. Second line should contains at least 3 characters '-' per column between the characters '|'.");
                                    ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.TableFormatError;
                                }
                            }
                        }
                    }

                    idxLine++;
                }
                else if (isMatch)
                {
                    // End of the table
                    idxLine = 0;
                    isMatch = false;
                }
            }
        }

        /// <summary>
        /// Process given file in give folder. Check all references.
        /// </summary>
        /// <param name="folder">Folder where file live.</param>
        /// <param name="filepath">Complete path of the file to check.</param>
        private static void ProcessFile(DirectoryInfo folder, string filepath)
        {
            string[] lines = File.ReadAllLines(filepath);

            // get matches per line to be able to reference a line in a file.
            int linenr = 1;
            foreach (string line in lines)
            {
                Regex rxLine = new Regex(@"(\[[^\]]+\]{1}\({1}[^\)]+\){1})");
                MatchCollection matches = rxLine.Matches(line);
                if (matches.Any())
                {
                    // process all matches
                    foreach (Match match in matches)
                    {
                        // get just the reference
                        int start = match.Value.IndexOf("](") + 2;
                        string relative = match.Value.Substring(start);
                        int end = relative.IndexOf(")");
                        relative = relative.Substring(0, end);
                        string afterSharp = string.Empty;

                        // relative string contain not only URL, but also "title", get rid of it
                        int positionOfLinkTitle = relative.IndexOf('\"');
                        if (positionOfLinkTitle > 0)
                        {
                            relative = relative.Substring(0, relative.IndexOf('\"')).Trim();
                        }

                        // strip in-doc references using a #
                        if (relative.Contains("#"))
                        {
                            // We keep the link after the sharp to check later on if it's a valid one
                            afterSharp = relative.Substring(relative.IndexOf("#") + 1);
                            relative = relative.Substring(0, relative.IndexOf("#"));
                        }

                        // decode possible HTML encoding
                        relative = HttpUtility.UrlDecode(relative);

                        // check link if not to a URL, in-doc link or e-mail address
                        if (!relative.StartsWith("http:") &&
                            !relative.StartsWith("https:") &&
                            !relative.Contains("@") &&
                            !string.IsNullOrEmpty(Path.GetExtension(relative)) &&
                            !string.IsNullOrWhiteSpace(relative))
                        {
                            // check validity of the link
                            string absolute;
                            if (relative.StartsWith("~/"))
                            {
                                // link is relative to project root directory
                                absolute = Path.GetFullPath(relative.Substring(2), rootDir.FullName);
                            }
                            else
                            {
                                // link is relative to its directory
                                absolute = Path.GetFullPath(relative, folder.FullName);
                            }

                            // check that paths are relative
                            if (Path.IsPathFullyQualified(relative))
                            {
                                // link is full path - not allowed
                                message.Output($"{filepath} {linenr}:{match.Index}");
                                message.Error($"Full path '{relative}' used. Use relative path.");
                                ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                            }

                            // don't need to check if reference is to a directory
                            if (!Directory.Exists(absolute))
                            {
                                // check if we have file in allFiles list or if it exists on disc
                                if (!allFiles.Contains(absolute.ToLowerInvariant()) && !File.Exists(absolute))
                                {
                                    // ERROR: link to non existing file
                                    message.Output($"{filepath} {linenr}:{match.Index}");
                                    message.Error($"Not found: {relative}");

                                    // mark error in exit code of the tool
                                    ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                                }
                                else
                                {
                                    if (!allLinks.Contains(absolute.ToLowerInvariant()))
                                    {
                                        // register reference unique in list
                                        allLinks.Add(absolute.ToLowerInvariant());
                                    }

                                    if (afterSharp != string.Empty)
                                    {
                                        // Time to check if the inside doc link is valid
                                        if (afterSharp.ToLower() != afterSharp)
                                        {
                                            // link is full path - not allowed
                                            message.Output($"{filepath} {linenr}:{match.Index}");
                                            message.Error($"Inside doc path '{relative}#{afterSharp}' must be lower case.");
                                            ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                                        }

                                        // We need to check if what is after the # is valid or not, first it must be lowercase.
                                        var fileContent = File.ReadAllLines(absolute);

                                        bool found = false;
                                        foreach (var lineTitle in fileContent)
                                        {
                                            // Find titles
                                            if (lineTitle.StartsWith('#'))
                                            {
                                                // Get rid of the title mark
                                                var lineTitleLink = lineTitle.Replace("#", string.Empty);

                                                // check if there is an HTML link ID like this: ## <a id=\"i_am_an_id\">Title for ID</a>
                                                Regex idLinkPattern = new Regex(@"(?><a id=\"")(?<id_label>[a-zA-Z -_]+)(?>\"">)");
                                                if (idLinkPattern.IsMatch(lineTitleLink))
                                                {
                                                    // we have a match for this pattern!

                                                    // check if the label id matches
                                                    if (idLinkPattern.Match(lineTitleLink).Groups["id_label"].Value == afterSharp)
                                                    {
                                                        found = true;
                                                        break;
                                                    }

                                                    // proceed with replacing the HTML link ID
                                                    lineTitleLink = idLinkPattern.Replace(lineTitleLink, string.Empty);

                                                    // and the closing link tag too
                                                    lineTitleLink = lineTitleLink.Replace("</a>", string.Empty);
                                                }

                                                // Remove the space
                                                lineTitleLink = lineTitleLink.TrimStart();

                                                // To lower
                                                lineTitleLink = lineTitleLink.ToLower();

                                                // Remove bold and italic as well as the . ? and few others
                                                lineTitleLink = lineTitleLink.Replace("*", string.Empty).Replace(".", string.Empty).Replace("'", string.Empty)
                                                    .Replace("\"", string.Empty).Replace("/", string.Empty).Replace("&", string.Empty)
                                                    .Replace("(", string.Empty).Replace(")", string.Empty).Replace("`", string.Empty).Replace("?", string.Empty);

                                                // Replave spaces by dash
                                                lineTitleLink = lineTitleLink.Replace(" ", "-");

                                                // If it our title?
                                                if (afterSharp == lineTitleLink)
                                                {
                                                    found = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!found)
                                        {
                                            // Let's check if it's an absolute link or not
                                            // Is it a link on a line number? Then pattern is 'l123456'
                                            uint numLines;
                                            Regex lineLinkPattern = new Regex(@"^l\d+");
                                            if (lineLinkPattern.IsMatch(afterSharp))
                                            {
                                                if (!uint.TryParse(afterSharp.Substring(1), out numLines))
                                                {
                                                    message.Output($"{filepath} {linenr}:{match.Index}");
                                                    message.Error($"In doc link was not found '{relative}#{afterSharp}'. Make sure you have all lowercase, remove '*' and replace spaces by '-'.");
                                                    ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                                                }
                                                else
                                                {
                                                    if (fileContent.Length < numLines)
                                                    {
                                                        message.Output($"{filepath} {linenr}:{match.Index}");
                                                        message.Error($"Line link is invalid '{relative}#{afterSharp}' must be less than the number of lines in the target file.");
                                                        ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                message.Output($"{filepath} {linenr}:{match.Index}");
                                                message.Error($"In doc link was not found '{relative}#{afterSharp}'. Make sure you have all lowercase, remove '*' and replace spaces by '-'.");
                                                ExitCodeHelper.ExitCode = ExitCodeHelper.ExitCodes.ParsingError;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                linenr++;
            }
        }
    }
}
