// Licensed to DocFX Companion Tools and contributors under one or more agreements.
// DocFX Companion Tools and contributors licenses this file to you under the MIT license.

namespace DocFxTocGenerator
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CommandLine;
    using DocFxTocGenerator.Domain;
    using DocFxTocGenerator.Helpers;
    using Microsoft.OpenApi.Readers;

    /// <summary>
    /// Toc generator.
    /// </summary>
    internal class TocGenerator
    {
        private static readonly string[] _filePatternsForToc = { "*.md", "*.swagger.json" };
        private static readonly string _filePatternsForTocJoined = string.Join(", ", _filePatternsForToc);
        private static readonly EnumerationOptions _caseSetting = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };

        private static CommandlineOptions _options;
        private static int _returnvalue;
        private static MessageHelper _message;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Commandline options described in <see cref="CommandlineOptions"/> class.</param>
        /// <returns>0 if succesful, 1 on error.</returns>
        private static int Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandlineOptions>(args)
                               .WithParsed<CommandlineOptions>(RunLogic)
                               .WithNotParsed(HandleErrors);

            Console.WriteLine($"Exit with return code {_returnvalue}");

            return _returnvalue;
        }

        /// <summary>
        /// Run the logic of the app with the given parameters.
        /// Given folders are checked if they exist.
        /// </summary>
        /// <param name="o">Parsed commandline options.</param>
        private static void RunLogic(CommandlineOptions o)
        {
            _options = o;
            _message = new MessageHelper(_options);

            if (string.IsNullOrEmpty(_options.OutputFolder))
            {
                _options.OutputFolder = _options.DocFolder;
            }

            _message.Verbose($"Documentation folder: {_options.DocFolder}");
            _message.Verbose($"Output folder       : {_options.OutputFolder}");
            _message.Verbose($"Verbose             : {_options.Verbose}");
            _message.Verbose($"Use .order          : {_options.UseOrder}");
            _message.Verbose($"Use .override       : {_options.UseOverride}");
            _message.Verbose($"Use .ignore         : {_options.UseIgnore}");
            _message.Verbose($"Auto index          : {_options.AutoIndex}\n");
            _message.Verbose($"Split toc depth     : {_options.SplitTocDepth}\n");

            if (!Directory.Exists(_options.DocFolder))
            {
                _message.Error($"ERROR: Documentation folder '{_options.DocFolder}' doesn't exist.");
                _returnvalue = 1;
                return;
            }

            if (!Directory.Exists(_options.OutputFolder))
            {
                _message.Error($"ERROR: Destination folder '{_options.OutputFolder}' doesn't exist.");
                _returnvalue = 1;
                return;
            }

            // we start at the root to generate the TOC items
            TocItem tocRootItems = new TocItem();
            DirectoryInfo rootDir = new DirectoryInfo(_options.DocFolder);
            WalkDirectoryTree(rootDir, tocRootItems);

            if (_options.SplitTocDepth > 0)
            {
                WriteChildTocItems(tocRootItems, string.Empty, 0);
            }
            else
            {
                // write the tocitems to disk as one large file
                WriteToc(tocRootItems, _options.OutputFolder);
            }
        }

        /// <summary>
        /// Walks the yaml tree looking for child nodes that are parents to other children
        /// and corrects the paths to be relative the number toc files that should be generated.
        /// </summary>
        /// <param name="parentTocItem">Parent toc item to walk.</param>
        /// <param name="parentFolder">Location where the toc should be written.</param>
        /// <param name="treeDepth">Indicates the level the toc item is at in the tree.</param>
        private static ICollection<TocItem> WriteChildTocItems(TocItem parentTocItem, string parentFolder, int treeDepth = 0)
        {
            var childTocItems = new TocItem();
            foreach (var tocItem in parentTocItem.Items.OrderBy(x => x.Sequence))
            {
                ICollection<TocItem> childItems = null;

                // split the href, may need the folder or filename later on
                var hrefParts = tocItem.Href.Split('/');

                // if the child is a leaf, use the href and remove the parent folder so it's relative
                // if the child has children then the href will point to the toc in the child folder
                var childHref = ((tocItem.Items?.Any() ?? false) && treeDepth < _options.SplitTocDepth)
                        ? string.Join('/', hrefParts.Take(hrefParts.Length == 1 ? 1 : hrefParts.Length - 1))
                        : tocItem.Href.Substring(parentFolder.Length).TrimStart('/');

                if (tocItem.Items?.Any() ?? false)
                {
                    childItems = WriteChildTocItems(tocItem, treeDepth < _options.SplitTocDepth ? childHref : parentFolder, treeDepth + 1);
                }

                childTocItems.AddItem(new TocItem()
                {
                    Title = tocItem.Title.Trim(),
                    Filename = tocItem.Filename,
                    Sequence = tocItem.Sequence,
                    SortableTitle = tocItem.SortableTitle,
                    Href = childItems != null && treeDepth < _options.SplitTocDepth ? null : childHref,
                    Items = childItems,
                });
            }

            if (treeDepth <= _options.SplitTocDepth)
            {
                WriteToc(childTocItems, Path.Combine(_options.OutputFolder, parentFolder));
                return null;
            }

            return childTocItems.Items;
        }

        private static void WriteToc(TocItem tocRootItems, string outputFolder)
        {
            // we have the TOC, so serialize to a string
            using (StringWriter sw = new StringWriter())
            {
                using (IndentedTextWriter writer = new IndentedTextWriter(sw, "  "))
                {
                    writer.WriteLine("# This is an automatically generated file");
                    Serialize(writer, tocRootItems, true);
                }

                // now write the TOC to disc
                File.WriteAllText(Path.Combine(outputFolder, "toc.yml"), sw.ToString());
            }

            _message.Verbose($"{Path.Combine(outputFolder, "toc.yml")} created.");
        }

        /// <summary>
        /// On parameter errors, we set the returnvalue to 1 to indicated an error.
        /// </summary>
        /// <param name="errors">List or errors (ignored).</param>
        private static void HandleErrors(IEnumerable<Error> errors)
        {
            _returnvalue = 1;
        }

        /// <summary>
        /// Main function going through all the folders, files and subfolders.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        /// <param name="yamlNode">An item.</param>
        /// <returns>Full path of the entry-file of this folder.</returns>
        private static string WalkDirectoryTree(DirectoryInfo folder, TocItem yamlNode)
        {
            _message.Verbose($"Processing folder {folder.FullName}");

            List<string> order = GetOrderList(folder);
            Dictionary<string, string> overrides = _options.UseOverride ? GetOverrides(folder) : new Dictionary<string, string>();
            List<string> ignore = GetIgnore(folder);

            // add doc files to the node
            GetFiles(folder, order, yamlNode, overrides);

            // add directories with content to the node
            GetDirectories(folder, order, yamlNode, overrides, ignore);

            if (yamlNode.Items != null)
            {
                // now sort the files and directories with the order-list and further alphabetically
                yamlNode.Items = new Collection<TocItem>(yamlNode.Items.OrderBy(x => x.Sequence).ThenBy(x => x.SortableTitle).ToList());
                _message.Verbose($"Items ordered in {folder.FullName}");
            }

            if (!string.IsNullOrWhiteSpace(yamlNode.Filename))
            {
                // if indicated, add a folder index - but not for the root folder.
                if (_options.AutoIndex)
                {
                    string indexFile = AddIndex(folder, yamlNode, GetOverrides(folder.Parent));
                    if (!string.IsNullOrEmpty(indexFile))
                    {
                        yamlNode.Href = GetRelativePath(indexFile, _options.DocFolder);
                    }
                }
                else
                {
                    if (yamlNode.Items != null && yamlNode.Items.Any())
                    {
                        yamlNode.Href = GetRelativePath(yamlNode.Items.First().Filename, _options.DocFolder);
                    }
                }
            }

            return yamlNode.Items == null ? string.Empty : yamlNode.Items.First().Filename;
        }

        private static List<string> GetIgnore(DirectoryInfo folder)
        {
            // see if we have an .order file
            List<string> ignore = new List<string>();
            if (_options.UseIgnore)
            {
                string orderFile = Path.Combine(folder.FullName, ".ignore");
                if (File.Exists(orderFile))
                {
                    _message.Verbose($"Read existing order file {orderFile}");
                    ignore = File.ReadAllLines(orderFile).ToList();
                }
            }

            return ignore;
        }

        /// <summary>
        /// Get the list of the files in the current directory.
        /// </summary>
        /// <param name="folder">The folder to search.</param>
        /// <param name="order">Order list.</param>
        /// <param name="yamlNode">The current toc node.</param>
        /// <param name="overrides">The overrides.</param>
        private static void GetFiles(DirectoryInfo folder, List<string> order, TocItem yamlNode, Dictionary<string, string> overrides)
        {
            _message.Verbose($"Process {folder.FullName} for files.");

            List<FileInfo> files =
                _filePatternsForToc
                .SelectMany(pattern => folder.GetFiles(pattern, _caseSetting))
                .OrderBy(f => f.Name)
                .ToList();
            if (!files.Any())
            {
                _message.Verbose($"No {_filePatternsForTocJoined} files found in {folder.FullName}.");
                return;
            }

            foreach (FileInfo fi in files)
            {
                if (fi.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // see if the file is mentioned in the order-list for ordering.
                int sequence = int.MaxValue;
                if (order.Contains(Path.GetFileNameWithoutExtension(fi.Name)))
                {
                    sequence = order.IndexOf(Path.GetFileNameWithoutExtension(fi.Name));
                }

                string title = string.Empty;
                if (_options.UseOverride && (overrides.Count > 0))
                {
                    // get possible title override from the .override file
                    var key = fi.Name.Substring(0, fi.Name.Length - 3);
                    if (overrides.ContainsKey(key))
                    {
                        title = overrides[key];
                    }
                }

                title = title.Length == 0 ? GetCleanedFileName(fi) : title;

                yamlNode.AddItem(new TocItem
                {
                    Sequence = sequence,
                    Filename = fi.FullName,
                    Title = title,
                    Href = GetRelativePath(fi.FullName, _options.DocFolder),
                });

                _message.Verbose($"Add file seq={sequence} title={title} href={GetRelativePath(fi.FullName, _options.DocFolder)}");
            }
        }

        /// <summary>
        /// Get the override file for given folder and process.
        /// </summary>
        /// <param name="folder">Current folder.</param>
        /// <returns>Dictionary containing overrides.</returns>
        private static Dictionary<string, string> GetOverrides(DirectoryInfo folder)
        {
            Dictionary<string, string> overrides = new Dictionary<string, string>();

            // Read the .override file
            string overrideFile = Path.Combine(folder.FullName, ".override");
            if (File.Exists(overrideFile))
            {
                _message.Verbose($"Read existing overrideFile file {overrideFile}");
                foreach (var over in File.ReadAllLines(overrideFile))
                {
                    var overSplit = over.Split(';');
                    if (overSplit?.Length == 2)
                    {
                        overrides.TryAdd(overSplit[0], overSplit[1]);
                    }
                }
            }

            _message.Verbose($"Found {overrides.Count} for folder {folder.FullName}");
            return overrides;
        }

        /// <summary>
        /// Walk through sub-folders and add if they contain content.
        /// </summary>
        /// <param name="folder">Folder to get sub-folders from.</param>
        /// <param name="order">Order list.</param>
        /// <param name="yamlNode">yamlNode to add entries to.</param>
        /// <param name="overrides">The overrides.</param>
        /// <param name="ignore">The ignore.</param>
        private static void GetDirectories(DirectoryInfo folder, List<string> order, TocItem yamlNode, Dictionary<string, string> overrides, List<string> ignore)
        {
            _message.Verbose($"Process {folder.FullName} for sub-directories.");

            // Now find all the subdirectories under this directory.
            DirectoryInfo[] subDirs = folder.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirs)
            {
                // skip hidden folders (starting with .)
                if (dirInfo.Name.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // If in the ignore file, then continue
                if (ignore.Contains(dirInfo.Name))
                {
                    continue;
                }

                // Get all the md files only
                FileInfo[] subFiles = _filePatternsForToc
                    .SelectMany(pattern => dirInfo.GetFiles(pattern, _caseSetting))
                    .ToArray();
                if (subFiles.Any() == false)
                {
                    _message.Warning($"WARNING: Folder {dirInfo.FullName} skipped as it doesn't contain {_filePatternsForTocJoined} files. This might skip further sub-folders. Solve this by adding a README.md or INDEX.md in the folder.");
                    continue;
                }

                TocItem newTocItem = new TocItem();

                // if the directory is in the .order file, take the index as sequence nr
                if (order.Contains(Path.GetFileName(dirInfo.Name)))
                {
                    newTocItem.Sequence = order.IndexOf(Path.GetFileName(dirInfo.Name));
                }

                string title = string.Empty;
                if (_options.UseOverride)
                {
                    // if in the .override file, override the title with it
                    if (overrides.ContainsKey(dirInfo.Name))
                    {
                        title = overrides[dirInfo.Name];
                    }
                }

                // Cleanup the title to be readable
                title = title.Length == 0 ? ToTitleCase(dirInfo.Name) : title;
                newTocItem.Filename = dirInfo.FullName;
                newTocItem.Title = title;
                string entryFile = string.Empty;
                if (!_options.NoAutoIndexWithOneFile)
                {
                    // when no extra indication, this will ALWAYS generate an INDEX.md
                    // if no index or readme exists. Independent of the number of files in the folder.
                    entryFile = WalkDirectoryTree(dirInfo, newTocItem);
                }

                if (subFiles.Length == 1 && dirInfo.GetDirectories().Length == 0)
                {
                    newTocItem.Href = GetRelativePath(subFiles[0].FullName, _options.DocFolder);
                }
                else
                {
                    if (_options.NoAutoIndexWithOneFile)
                    {
                        // when this extra indication set, this will ONLY generate an INDEX.md
                        // if no index or readme exists and if the folder contains more than 1 file.
                        entryFile = WalkDirectoryTree(dirInfo, newTocItem);
                    }

                    newTocItem.Href = GetRelativePath(entryFile, _options.DocFolder);
                }

                _message.Verbose($"Add directory seq={newTocItem.Sequence} title={newTocItem.Title} href={newTocItem.Href}");

                yamlNode.AddItem(newTocItem);
            }
        }

        /// <summary>
        /// Get an order-list for ordering items in the given folder.
        /// We'll try to read the '.order' file. If that one exists, it's read.
        /// We'll always add readme to the order-list if they weren't added yet.
        /// This make sure that even when the .order is not set, the readme gets a prominent position.
        /// </summary>
        /// <param name="folder">Folder to check for .order file.</param>
        /// <returns>Ordered list.</returns>
        private static List<string> GetOrderList(DirectoryInfo folder)
        {
            // see if we have an .order file
            List<string> order = new List<string>();
            if (_options.UseOrder)
            {
                string orderFile = Path.Combine(folder.FullName, ".order");
                if (File.Exists(orderFile))
                {
                    _message.Verbose($"Read existing order file {orderFile}");
                    order = File.ReadAllLines(orderFile).ToList();
                }
            }

            // we always want to order README.md. So add it if not in list yet
            string readmeEntry = order.FirstOrDefault(x => string.Equals("README", x, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(readmeEntry))
            {
                order.Add("README");
                _message.Verbose($"'README' added to order-list");
            }

            // we always want to order INDEX.md as well. So add it if not in list yet
            string indexEntry = order.FirstOrDefault(x => string.Equals("index", x, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(indexEntry))
            {
                order.Add("index");
                _message.Verbose($"'index' added to order-list");
            }

            return order;
        }

        /// <summary>
        /// See if there is a 1) README.md or 2) index.md.
        /// If those don't exist, we add a standard index.md.
        /// </summary>
        /// <param name="folder">Folder to work with.</param>
        /// <param name="yamlNode">Toc item for the folder.</param>
        /// <param name="overrides">The overrides.</param>
        /// <returns>Name of the file where the index was added.</returns>
        private static string AddIndex(DirectoryInfo folder, TocItem yamlNode, Dictionary<string, string> overrides)
        {
            // don't add index if there is nothing to index
            if (yamlNode.Items == null || !yamlNode.Items.Any())
            {
                return string.Empty;
            }

            // determine output file. Standard is index.md.
            string readmeFile = Path.Combine(folder.FullName, "README.md");
            string indexFile = Path.Combine(folder.FullName, "index.md");

            // if one of these files exists, we don't add an index.
            if (File.Exists(readmeFile) || File.Exists(indexFile))
            {
                return string.Empty;
            }

            if (WriteIndex(indexFile, yamlNode))
            {
                // if a new index has been created, add that to the TOC (top of list)
                FileInfo fi = folder.GetFiles().FirstOrDefault(x => string.Equals(x.Name, Path.GetFileName(indexFile), StringComparison.OrdinalIgnoreCase));
                string title = string.Empty;
                if (_options.UseOverride && (overrides.Count > 0))
                {
                    string key = folder.Name;
                    if (overrides.ContainsKey(key))
                    {
                        title = overrides[key];
                    }
                }

                TocItem newItem = new TocItem
                {
                    Sequence = -1,
                    Filename = indexFile,
                    Title = title.Length == 0 ? GetCleanedFileName(fi) : title,
                    Href = GetRelativePath(indexFile, _options.DocFolder),
                };

                // insert index item at the top
                yamlNode.AddItem(newItem, true);
                _message.Verbose($"Added index.md to top of list of files.");
            }

            return indexFile;
        }

        /// <summary>
        /// Write the index in the given outputFile.
        /// </summary>
        /// <param name="outputFile">File to write to.</param>
        /// <param name="yamlNode">TOC Item to get files from.</param>
        /// <returns>Was a NEW index file created TRUE/FALSE.</returns>
        private static bool WriteIndex(string outputFile, TocItem yamlNode)
        {
            if (File.Exists(outputFile))
            {
                return false;
            }

            _message.Verbose($"Index will be written to {outputFile}");

            // read lines if existing file.
            List<string> lines = new List<string>();

            lines.Add($"# {yamlNode.Title}");
            lines.Add(string.Empty);
            foreach (TocItem item in yamlNode.Items)
            {
                lines.Add($"* [{item.Title}](./{Path.GetFileName(item.Filename)})");
            }

            File.WriteAllLines(outputFile, lines);
            _message.Verbose($"Written {lines.Count} lines to {outputFile} (index).");
            return true;
        }

        /// <summary>
        /// Serialize a toc item.
        /// </summary>
        /// <param name="writer">Writer to use for output.</param>
        /// <param name="tocItem">the toc item to serialize.</param>
        /// <param name="isRoot">Is this the root, then we don't indent extra.</param>
        /// <remarks>The representation is like this:
        /// items:
        /// - name: Document One
        ///   href: document-one.md
        /// - name: Document Two
        ///   href: document-two.md
        /// - name: Sub Folder
        ///   href: sub-folder/index.md
        ///   items:
        ///   - name: Index
        ///     href: sub-folder/index.md
        ///   - name: Sub Document One
        ///     href: sub-folder/sub-document-one.md.
        /// </remarks>
        private static void Serialize(IndentedTextWriter writer, TocItem tocItem, bool isRoot = false)
        {
            if (!string.IsNullOrEmpty(tocItem.Title))
            {
                writer.WriteLine($"- name: {tocItem.Title}");
            }

            if (!string.IsNullOrEmpty(tocItem.Href))
            {
                writer.WriteLine($"  href: {tocItem.Href}");
            }

            if (tocItem.Items != null)
            {
                if (!isRoot)
                {
                    writer.Indent++;
                }

                writer.WriteLine("items:");
                foreach (TocItem toc in tocItem.Items)
                {
                    Serialize(writer, toc);
                }

                if (!isRoot)
                {
                    writer.Indent--;
                }
            }
        }

        /// <summary>
        /// Clean the file names.
        /// </summary>
        /// <param name="fi">A file name.</param>
        /// <returns>A cleaned string replacing - and _ and removing md extension as well as non authorized characters.</returns>
        private static string GetCleanedFileName(FileInfo fi)
        {
            string cleanedName = fi.Name;
            if (string.Equals(fi.Name, "INDEX.MD", StringComparison.OrdinalIgnoreCase))
            {
                // if this is the index doc, give it the name of the folder.
                cleanedName = Path.GetFileName(fi.DirectoryName);
            }
            else if (fi.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                // For markdownfile, open the file, read the line up to the first #, extract the tile
                using (StreamReader toRead = File.OpenText(fi.FullName))
                {
                    while (!toRead.EndOfStream)
                    {
                        string strTitle = toRead.ReadLine();
                        if (strTitle.TrimStart(' ').StartsWith("# ", StringComparison.OrdinalIgnoreCase))
                        {
                            cleanedName = strTitle.Substring(2);
                            break;
                        }
                    }
                }
            }
            else if (fi.Name.EndsWith(".swagger.json", StringComparison.OrdinalIgnoreCase))
            {
                // for open api swagger file, read the title from the data.
                using var stream = File.OpenRead(fi.FullName);
                var document = new OpenApiStreamReader().Read(stream, out _);
                cleanedName = $"{document.Info.Title} {document.Info.Version}";
            }

            return ToTitleCase(cleanedName);
        }

        /// <summary>
        /// Uppercase first character and remove unwanted characters.
        /// </summary>
        /// <param name="title">The name to clean.</param>
        /// <returns>A clean name.</returns>
        private static string ToTitleCase(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return string.Empty;
            }

            string cleantitle = title.First().ToString().ToUpperInvariant() + title.Substring(1);
            cleantitle = Regex.Replace(cleantitle, @"[-_+]", " ");
            return Regex.Replace(cleantitle, @"([\[\]\:`\\{}()#\*]|\.md)", string.Empty);
        }

        /// <summary>
        /// Get the relative path.
        /// </summary>
        /// <param name="filePath">The file to get the relative path.</param>
        /// <param name="sourcePath">The source path, by default the current directory.</param>
        private static string GetRelativePath(string filePath, string sourcePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            string currentDir = sourcePath ?? Environment.CurrentDirectory;
            return Path.GetRelativePath(Path.GetFullPath(currentDir), filePath)
                .Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
