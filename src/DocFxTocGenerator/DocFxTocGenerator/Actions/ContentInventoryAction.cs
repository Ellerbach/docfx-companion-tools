// <copyright file="ContentInventoryAction.cs" company="DocFx Companion Tools">
// Copyright (c) DocFx Companion Tools. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;
using Microsoft.Extensions.Logging;

namespace DocFxTocGenerator.Actions;

/// <summary>
/// Get the content from the root documentation folder for processing.
/// This action only retrieves information that can be used in other
/// steps of the process to generate index files or the table of contents.
/// </summary>
public class ContentInventoryAction
{
    private readonly string _docsFolder;
    private readonly bool _useOrder;
    private readonly bool _useIgnore;
    private readonly bool _useOverride;
    private readonly bool _camelCasing;

    private readonly IFileService? _fileService;
    private readonly ConfigFilesService _configService;
    private readonly ILogger _logger;

    private readonly FileInfoService _fileDataService;

    private List<string> _mdFiles = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentInventoryAction"/> class.
    /// </summary>
    /// <param name="docsFolder">Documentation folder.</param>
    /// <param name="useOrder">Use the .order configuration per directory.</param>
    /// <param name="useIgnore">Use the .ignore configuration per directory.</param>
    /// <param name="useOverride">Use the .override configuration per directory.</param>
    /// <param name="camelCasing">Use camel casing for titles.</param>
    /// <param name="fileService">File service.</param>
    /// <param name="logger">Logger.</param>
    public ContentInventoryAction(
        string docsFolder,
        bool useOrder,
        bool useIgnore,
        bool useOverride,
        bool camelCasing,
        IFileService fileService,
        ILogger logger)
    {
        _docsFolder = docsFolder;

        _useOrder = useOrder;
        _useIgnore = useIgnore;
        _useOverride = useOverride;
        _camelCasing = camelCasing;

        _fileService = fileService;
        _logger = logger;
        _configService = new(camelCasing, fileService, logger);
        _fileDataService = new(camelCasing, fileService, logger);
    }

    /// <summary>
    /// Gets root folder information after action has run.
    /// </summary>
    public FolderData? RootFolder { get; private set; }

    /// <summary>
    /// Run the action. Result is accessible through the <see cref="RootFolder"/> property.
    /// </summary>
    /// <returns>0 on success, 1 on warning, 2 on error.</returns>
    public Task<ReturnCode> RunAsync()
    {
        ReturnCode ret = ReturnCode.Normal;
        _logger.LogInformation($"\n*** INVENTORY STAGE.");

        // find all markdown files
        _mdFiles = _fileService!.GetFiles(_docsFolder, ["**/*.md", "**/*swagger.json"], []).ToList();
        _logger.LogInformation($"Found {_mdFiles.Count} content files in '{_docsFolder}'");

        try
        {
            RootFolder = GetFolderData(null, _docsFolder);
            if (RootFolder == null)
            {
                ret = ReturnCode.Warning;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Inventory error: {ex.Message}.");
            ret = ReturnCode.Error;
        }

        _logger.LogInformation($"END OF INVENTORY STAGE. Result: {ret}");
        return Task.FromResult(ret);
    }

    private FolderData? GetFolderData(FolderData? parent, string dirpath)
    {
        if (!_fileService!.ExistsFileOrDirectory(dirpath))
        {
            throw new ActionException($"ERROR: folder '{dirpath}' doesn't exist!");
        }

        // check if we have any markdown file in the given folder or it's subfolders
        if (_mdFiles.FirstOrDefault(x => x.StartsWith(dirpath.NormalizePath(), StringComparison.OrdinalIgnoreCase)) == null)
        {
            // if not, we can skip this folder.
            _logger.LogInformation($"No content files found in '{dirpath}'");
            return null;
        }

        // set basic folder information
        FolderData? folder = new()
        {
            Name = Path.GetFileName(dirpath),
            DisplayName = _fileDataService.ToTitleCase(Path.GetFileNameWithoutExtension(dirpath), _camelCasing),
            Path = dirpath.NormalizePath(),
            Parent = parent,
        };

        if (parent != null)
        {
            // see if we have an override for the folder name
            if (parent.OverrideList.TryGetValue(folder.Name, out string? name))
            {
                folder.DisplayName = name;
                folder.IsDisplayNameOverride = true;
            }
        }

        // read config files
        if (_useOrder)
        {
            folder.OrderList = _configService.GetOrderList(dirpath);
        }
        else
        {
            folder.OrderList = ConfigFilesService.DefaultOrderList;
        }

        if (_useIgnore)
        {
            folder.IgnoreList = _configService.GetIgnoreList(dirpath);
        }

        if (_useOverride)
        {
            folder.OverrideList = _configService.GetOverrideList(dirpath);
        }

        // add files in this folder
        AddFiles(folder, dirpath);

        // add folders in this folder
        AddFolders(folder, dirpath);

        return folder;
    }

    private void AddFiles(FolderData folder, string dirPath)
    {
        string[] patterns = ["*.md", "*.swagger.json"];
        string patternsJoined = string.Join(", ", patterns);
        EnumerationOptions caseSetting = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };

        // get files
        var files = _fileService!.GetFiles(dirPath, ["*.md", "*swagger.json"], []);
        _logger.LogInformation($"Processing {files.Count()} files in '{dirPath}'");

        foreach (var file in files)
        {
            folder.Files.Add(_fileDataService.CreateFileData(folder, file));
        }

        // order on sequence and display name
        folder.Files = folder.Files.OrderBy(x => x.Sequence).ThenBy(x => x.DisplayName).ToList();
        _logger.LogInformation($"Found {folder.FileCount} files in '{dirPath}' to add.");
    }

    private void AddFolders(FolderData folder, string dirPath)
    {
        var subdirs = _fileService!.GetDirectories(dirPath);

        if (subdirs.Any())
        {
            _logger.LogInformation($"Processing {subdirs.Count()} directories in '{dirPath}'");
        }

        foreach (var subdir in subdirs)
        {
            if (!folder.IgnoreList.Contains(Path.GetFileName(subdir)))
            {
                var subfolder = GetFolderData(folder, subdir);
                if (subfolder != null)
                {
                    folder.Folders.Add(subfolder);
                }
            }
            else
            {
                _logger.LogInformation($"Skipping sub-directory '{subdir}' as it is marked as such in the .ignore file.");
            }
        }

        // order on sequence and display name
        folder.Folders = folder.Folders.OrderBy(x => x.Sequence).ThenBy(x => x.DisplayName).ToList();
        _logger.LogInformation($"Found {folder.FolderCount} sub-directories in '{dirPath}' to add.");
    }
}
