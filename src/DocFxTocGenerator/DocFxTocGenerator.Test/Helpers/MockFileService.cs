using System.Diagnostics;
using System.Text;
using DocFxTocGenerator.ConfigFiles;
using DocFxTocGenerator.FileService;

namespace DocFxTocGenerator.Test.Helpers;

public class MockFileService : IFileService
{
    public string Root = "d:\\Git\\Project\\docs";

    public Dictionary<string, string> Files { get; set; } = new();

    public void FillDemoSet()
    {
        Files.Clear();

        AddFile(string.Empty, "README.md", string.Empty
            .AddHeading("Main readme", 1)
            .AddParagraphs(3));

        var folder = AddFolder($"continents");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Continents README", 1)
            .AddParagraphs(3));
        AddFile(folder, "unmentioned-continents.md", string.Empty
            .AddHeading("Unmentioned Continents", 1)
            .AddParagraphs(2));

        folder = AddFolder($"continents/americas");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("The Americas", 1)
            .AddParagraphs(1));

        folder = AddFolder($"continents/americas/brasil");
        AddFile(folder, "nova-friburgo.md", string.Empty
            .AddHeading("Nova Friburgo", 1)
            .AddParagraphs(2));
        AddFile(folder, "rio-de-janeirio.md", string.Empty
            .AddHeading("Rio de Janeiro", 1)
            .AddParagraphs(2));
        AddFile(folder, "sao-paulo.md", string.Empty
            .AddHeading("Sao Paulo", 1)
            .AddParagraphs(2));
        AddFile(folder, ".order",
@"sao-paulo
nova-friburgo
non-existing
rio-de-janeiro");

        folder = AddFolder($"continents/americas/united-states");
        // leave folder empty.
        folder = AddFolder($"continents/americas/united-states/new-york");
        AddFile(folder, "new-york-city.md", string.Empty
            .AddHeading("New York City", 1)
            .AddParagraphs(2));
        folder = AddFolder($"continents/americas/united-states/california");
        AddFile(folder, "san-francisco.md", string.Empty
            .AddHeading("San Francisco", 1)
            .AddParagraphs(2));
        AddFile(folder, "los-angeles.md", string.Empty
            .AddHeading("Los Angeles", 1)
            .AddParagraphs(2));
        AddFile(folder, "san-diego.md", string.Empty
            .AddHeading("San Diego", 1)
            .AddParagraphs(2));
        AddFile(folder, ".ignore",
@"los-angeles");

        folder = AddFolder($"continents/americas/united-states/washington");
        AddFile(folder, "seattle.md", string.Empty
            .AddHeading("Seattle", 1)
            .AddParagraphs(2));
        AddFile(folder, "tacoma.md", string.Empty
            .AddHeading("Tacoma", 1)
            .AddParagraphs(2));
        AddFile(folder, ".override",
@"tacoma;This is where the airport is - Tacoma Airport");

        folder = AddFolder($"continents/europe");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Europe", 1)
            .AddParagraphs(1));

        folder = AddFolder($"continents/europe/germany");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Germany README", 1)
            .AddParagraphs(1));
        AddFile(folder, "berlin.md", string.Empty
            .AddHeading("Berlin", 1)
            .AddParagraphs(1));
        AddFile(folder, "munchen.md", string.Empty
            .AddHeading("München", 1)
            .AddParagraphs(1));

        folder = AddFolder($"continents/europe/netherlands");
        // leave folder empty
        folder = AddFolder($"continents/europe/netherlands/zuid-holland");
        AddFile(folder, "rotterdam.md", string.Empty
            .AddHeading("Rotterdam", 1)
            .AddParagraphs(3));
        AddFile(folder, "den-haag.md", string.Empty
            .AddHeading("The Hague", 1)
            .AddParagraphs(3));
        folder = AddFolder($"continents/europe/netherlands/noord-holland");
        AddFile(folder, "amsterdam.md", string.Empty
            .AddHeading("Amsterdam", 1)
            .AddParagraphs(3));

        folder = AddFolder("deep-tree");
        folder = AddFolder("deep-tree/level1");
        folder = AddFolder("deep-tree/level1/level2");
        AddFile(folder, "index.md", string.Empty
            .AddHeading("Index of LEVEL 2", 1)
            .AddParagraphs(1));
        folder = AddFolder("deep-tree/level1/level2/level3");
        folder = AddFolder("deep-tree/level1/level2/level3/level4");
        folder = AddFolder("deep-tree/level1/level2/level3/level4/level5");
        AddFile(folder, "README.md", string.Empty
            .AddHeading("Deep tree readme", 1)
            .AddParagraphs(5));

        folder = AddFolder("software");
        folder = AddFolder("software/apis");
        folder = AddFolder("software/apis/test-api");
        #region adding test-api.swagger.json
        AddFile(folder, "test-api.swagger.json",
@"{
    ""swagger"": ""2.0"",
    ""info"": {
      ""title"": ""feature.proto"",
      ""version"": ""1.0.0.3145""
    },
    ""schemes"": [
      ""http"",
      ""https""
    ],
    ""consumes"": [
      ""application/json""
    ],
    ""produces"": [
      ""application/json""
    ],
    ""paths"": {
      ""/ops/features"": {
        ""get"": {
          ""summary"": ""GET /ops/features"",
          ""operationId"": ""ListFeatures"",
          ""responses"": {
            ""200"": {
              ""description"": """",
              ""schema"": {
                ""$ref"": ""#/definitions/featureListFeaturesResponse""
              }
            }
          },
          ""tags"": [
            ""Generic""
          ]
        }
      },
      ""/ops/features/{uuid}"": {
        ""put"": {
          ""summary"": ""PUT /ops/features/:uuid"",
          ""operationId"": ""UpdateFeature"",
          ""responses"": {
            ""200"": {
              ""description"": """",
              ""schema"": {
                ""$ref"": ""#/definitions/featureUpdateFeatureResponse""
              }
            }
          },
          ""parameters"": [
            {
              ""name"": ""uuid"",
              ""in"": ""path"",
              ""required"": true,
              ""type"": ""string"",
              ""format"": ""string""
            },
            {
              ""name"": ""body"",
              ""in"": ""body"",
              ""required"": true,
              ""schema"": {
                ""$ref"": ""#/definitions/protobufStruct""
              }
            }
          ],
          ""tags"": [
            ""Generic""
          ]
        }
      }
    },
    ""definitions"": {
      ""featureFeature"": {
        ""type"": ""object"",
        ""properties"": {
          ""app_id"": {
            ""type"": ""string"",
            ""format"": ""string""
          },
          ""args"": {
            ""type"": ""array"",
            ""items"": {
              ""type"": ""string"",
              ""format"": ""string""
            }
          },
          ""enabled"": {
            ""type"": ""boolean"",
            ""format"": ""boolean""
          },
          ""extra_data"": {
            ""$ref"": ""#/definitions/protobufStruct"",
            ""title"": ""extra data can be any json data\nFor parnter(特权专区) feature it shold be\nrepeated PartnerExtra extra_data = 10""
          },
          ""method"": {
            ""$ref"": ""#/definitions/featureFeatureFetchMethod""
          },
          ""requirements"": {
            ""type"": ""array"",
            ""items"": {
              ""type"": ""string"",
              ""format"": ""string""
            }
          },
          ""type"": {
            ""$ref"": ""#/definitions/featureFeatureType""
          },
          ""updated_at"": {
            ""type"": ""string"",
            ""format"": ""date-time""
          },
          ""uuid"": {
            ""type"": ""string"",
            ""format"": ""string""
          }
        }
      },
      ""featureFeatureFetchMethod"": {
        ""type"": ""string"",
        ""enum"": [
          ""UNKNOWN_METHOD"",
          ""GET""
        ],
        ""default"": ""UNKNOWN_METHOD""
      },
      ""featureFeatureType"": {
        ""type"": ""string"",
        ""enum"": [
          ""UNKNOWN_FEATURE_TYPE"",
          ""URL""
        ],
        ""default"": ""UNKNOWN_FEATURE_TYPE""
      },
      ""featureListFeaturesRequest"": {
        ""type"": ""object""
      },
      ""featureListFeaturesResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""features"": {
            ""type"": ""array"",
            ""items"": {
              ""$ref"": ""#/definitions/featureFeature""
            }
          }
        }
      },
      ""featureUpdateFeatureRequest"": {
        ""type"": ""object"",
        ""properties"": {
          ""extra_data"": {
            ""$ref"": ""#/definitions/protobufStruct""
          },
          ""token"": {
            ""type"": ""string"",
            ""format"": ""string""
          },
          ""uuid"": {
            ""type"": ""string"",
            ""format"": ""string""
          }
        }
      },
      ""featureUpdateFeatureResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""feature"": {
            ""$ref"": ""#/definitions/featureFeature""
          }
        }
      },
      ""protobufListValue"": {
        ""type"": ""object"",
        ""properties"": {
          ""values"": {
            ""type"": ""array"",
            ""items"": {
              ""$ref"": ""#/definitions/protobufValue""
            },
            ""description"": ""Repeated field of dynamically typed values.""
          }
        },
        ""description"": ""`ListValue` is a wrapper around a repeated field of values.\n\nThe JSON representation for `ListValue` is JSON array.""
      },
      ""protobufNullValue"": {
        ""type"": ""string"",
        ""enum"": [
          ""NULL_VALUE""
        ],
        ""default"": ""NULL_VALUE"",
        ""description"": ""`NullValue` is a singleton enumeration to represent the null value for the\n`Value` type union.\n\n The JSON representation for `NullValue` is JSON `null`.\n\n - NULL_VALUE: Null value.""
      },
      ""protobufStruct"": {
        ""type"": ""object"",
        ""properties"": {
          ""fields"": {
            ""type"": ""object"",
            ""additionalProperties"": {
              ""$ref"": ""#/definitions/protobufValue""
            },
            ""description"": ""Unordered map of dynamically typed values.""
          }
        },
        ""description"": ""`Struct` represents a structured data value, consisting of fields\nwhich map to dynamically typed values. In some languages, `Struct`\nmight be supported by a native representation. For example, in\nscripting languages like JS a struct is represented as an\nobject. The details of that representation are described together\nwith the proto support for the language.\n\nThe JSON representation for `Struct` is JSON object.""
      },
      ""protobufValue"": {
        ""type"": ""object"",
        ""properties"": {
          ""bool_value"": {
            ""type"": ""boolean"",
            ""format"": ""boolean"",
            ""description"": ""Represents a boolean value.""
          },
          ""list_value"": {
            ""$ref"": ""#/definitions/protobufListValue"",
            ""description"": ""Represents a repeated `Value`.""
          },
          ""null_value"": {
            ""$ref"": ""#/definitions/protobufNullValue"",
            ""description"": ""Represents a null value.""
          },
          ""number_value"": {
            ""type"": ""number"",
            ""format"": ""double"",
            ""description"": ""Represents a double value.""
          },
          ""string_value"": {
            ""type"": ""string"",
            ""format"": ""string"",
            ""description"": ""Represents a string value.""
          },
          ""struct_value"": {
            ""$ref"": ""#/definitions/protobufStruct"",
            ""description"": ""Represents a structured value.""
          }
        },
        ""description"": ""`Value` represents a dynamically typed value which can be either\nnull, a number, a string, a boolean, a recursive struct value, or a\nlist of values. A producer of value is expected to set one of that\nvariants, absence of any variant indicates an error.\n\nThe JSON representation for `Value` is JSON value.""
      }
    }
  }");
        #endregion
        folder = AddFolder("software/apis/test-plain-api");
        #region swagger.json
        AddFile(folder, "swagger.json",
@"{
    ""swagger"": ""2.0"",
    ""info"": {
      ""title"": ""SimpleApi.Test""
    },
    ""schemes"": [
      ""http"",
      ""https""
    ],
    ""consumes"": [
      ""application/json""
    ],
    ""produces"": [
      ""application/json""
    ],
    ""paths"": {
      ""/ops/features"": {
        ""get"": {
          ""summary"": ""GET /ops/features"",
          ""operationId"": ""ListFeatures"",
          ""responses"": {
            ""200"": {
              ""description"": """",
              ""schema"": {
                ""$ref"": ""#/definitions/featureListFeaturesResponse""
              }
            }
          },
          ""tags"": [
            ""Generic""
          ]
        }
      },
      ""/ops/features/{uuid}"": {
        ""put"": {
          ""summary"": ""PUT /ops/features/:uuid"",
          ""operationId"": ""UpdateFeature"",
          ""responses"": {
            ""200"": {
              ""description"": """",
              ""schema"": {
                ""$ref"": ""#/definitions/featureUpdateFeatureResponse""
              }
            }
          },
          ""parameters"": [
            {
              ""name"": ""uuid"",
              ""in"": ""path"",
              ""required"": true,
              ""type"": ""string"",
              ""format"": ""string""
            },
            {
              ""name"": ""body"",
              ""in"": ""body"",
              ""required"": true,
              ""schema"": {
                ""$ref"": ""#/definitions/protobufStruct""
              }
            }
          ],
          ""tags"": [
            ""Generic""
          ]
        }
      }
    },
    ""definitions"": {
      ""featureFeature"": {
        ""type"": ""object"",
        ""properties"": {
          ""app_id"": {
            ""type"": ""string"",
            ""format"": ""string""
          },
          ""args"": {
            ""type"": ""array"",
            ""items"": {
              ""type"": ""string"",
              ""format"": ""string""
            }
          },
          ""enabled"": {
            ""type"": ""boolean"",
            ""format"": ""boolean""
          },
          ""extra_data"": {
            ""$ref"": ""#/definitions/protobufStruct"",
            ""title"": ""extra data can be any json data\nFor parnter(特权专区) feature it shold be\nrepeated PartnerExtra extra_data = 10""
          },
          ""method"": {
            ""$ref"": ""#/definitions/featureFeatureFetchMethod""
          },
          ""requirements"": {
            ""type"": ""array"",
            ""items"": {
              ""type"": ""string"",
              ""format"": ""string""
            }
          },
          ""type"": {
            ""$ref"": ""#/definitions/featureFeatureType""
          },
          ""updated_at"": {
            ""type"": ""string"",
            ""format"": ""date-time""
          },
          ""uuid"": {
            ""type"": ""string"",
            ""format"": ""string""
          }
        }
      },
      ""featureFeatureFetchMethod"": {
        ""type"": ""string"",
        ""enum"": [
          ""UNKNOWN_METHOD"",
          ""GET""
        ],
        ""default"": ""UNKNOWN_METHOD""
      },
      ""featureFeatureType"": {
        ""type"": ""string"",
        ""enum"": [
          ""UNKNOWN_FEATURE_TYPE"",
          ""URL""
        ],
        ""default"": ""UNKNOWN_FEATURE_TYPE""
      },
      ""featureListFeaturesRequest"": {
        ""type"": ""object""
      },
      ""featureListFeaturesResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""features"": {
            ""type"": ""array"",
            ""items"": {
              ""$ref"": ""#/definitions/featureFeature""
            }
          }
        }
      },
      ""featureUpdateFeatureRequest"": {
        ""type"": ""object"",
        ""properties"": {
          ""extra_data"": {
            ""$ref"": ""#/definitions/protobufStruct""
          },
          ""token"": {
            ""type"": ""string"",
            ""format"": ""string""
          },
          ""uuid"": {
            ""type"": ""string"",
            ""format"": ""string""
          }
        }
      },
      ""featureUpdateFeatureResponse"": {
        ""type"": ""object"",
        ""properties"": {
          ""feature"": {
            ""$ref"": ""#/definitions/featureFeature""
          }
        }
      },
      ""protobufListValue"": {
        ""type"": ""object"",
        ""properties"": {
          ""values"": {
            ""type"": ""array"",
            ""items"": {
              ""$ref"": ""#/definitions/protobufValue""
            },
            ""description"": ""Repeated field of dynamically typed values.""
          }
        },
        ""description"": ""`ListValue` is a wrapper around a repeated field of values.\n\nThe JSON representation for `ListValue` is JSON array.""
      },
      ""protobufNullValue"": {
        ""type"": ""string"",
        ""enum"": [
          ""NULL_VALUE""
        ],
        ""default"": ""NULL_VALUE"",
        ""description"": ""`NullValue` is a singleton enumeration to represent the null value for the\n`Value` type union.\n\n The JSON representation for `NullValue` is JSON `null`.\n\n - NULL_VALUE: Null value.""
      },
      ""protobufStruct"": {
        ""type"": ""object"",
        ""properties"": {
          ""fields"": {
            ""type"": ""object"",
            ""additionalProperties"": {
              ""$ref"": ""#/definitions/protobufValue""
            },
            ""description"": ""Unordered map of dynamically typed values.""
          }
        },
        ""description"": ""`Struct` represents a structured data value, consisting of fields\nwhich map to dynamically typed values. In some languages, `Struct`\nmight be supported by a native representation. For example, in\nscripting languages like JS a struct is represented as an\nobject. The details of that representation are described together\nwith the proto support for the language.\n\nThe JSON representation for `Struct` is JSON object.""
      },
      ""protobufValue"": {
        ""type"": ""object"",
        ""properties"": {
          ""bool_value"": {
            ""type"": ""boolean"",
            ""format"": ""boolean"",
            ""description"": ""Represents a boolean value.""
          },
          ""list_value"": {
            ""$ref"": ""#/definitions/protobufListValue"",
            ""description"": ""Represents a repeated `Value`.""
          },
          ""null_value"": {
            ""$ref"": ""#/definitions/protobufNullValue"",
            ""description"": ""Represents a null value.""
          },
          ""number_value"": {
            ""type"": ""number"",
            ""format"": ""double"",
            ""description"": ""Represents a double value.""
          },
          ""string_value"": {
            ""type"": ""string"",
            ""format"": ""string"",
            ""description"": ""Represents a string value.""
          },
          ""struct_value"": {
            ""$ref"": ""#/definitions/protobufStruct"",
            ""description"": ""Represents a structured value.""
          }
        },
        ""description"": ""`Value` represents a dynamically typed value which can be either\nnull, a number, a string, a boolean, a recursive struct value, or a\nlist of values. A producer of value is expected to set one of that\nvariants, absence of any variant indicates an error.\n\nThe JSON representation for `Value` is JSON value.""
      }
    }
  }");
        #endregion
    }

    public string AddFolder(string relativePath)
    {
        var fullPath = Path.Combine(Root, relativePath).ToInternal();
        Files.Add(fullPath, string.Empty);
        return relativePath;
    }

    public void AddFile(string folderPath, string filename, string content)
    {
        var fullPath = Path.Combine(Root, folderPath, filename).ToInternal();
        Files.Add(fullPath, content);
    }

    public (string Path, string Content) GetOrderFile()
    {
        var file = Files.First(x => x.Key.EndsWith(".order"));
        return (file.Key, file.Value);
    }

    public (string Path, string Content) GetIgnoreFile()
    {
        var file = Files.First(x => x.Key.EndsWith(".ignore"));
        return (file.Key, file.Value);
    }

    public (string Path, string Content) GetOverrideFile()
    {
        var file = Files.First(x => x.Key.EndsWith(".override"));
        return (file.Key, file.Value);
    }

    public (string Path, string Content) GetNormalSwaggerFile()
    {
        var file = Files.First(x => x.Key.EndsWith(".swagger.json"));
        return (file.Key, file.Value);
    }

    public (string Path, string Content) GetSimpleSwaggerFile()
    {
        var file = Files.First(x => x.Key.EndsWith("/swagger.json"));
        return (file.Key, file.Value);
    }

    public void Delete(string path)
    {
        Files.Remove(GetFullPath(path.ToInternal()));
    }

    public void Delete(string[] paths)
    {
        foreach (var path in paths)
        {
            Files.Remove(GetFullPath(path).ToInternal());
        }
    }

    public bool ExistsFileOrDirectory(string path)
    {
        return Files.ContainsKey(GetFullPath(path).ToInternal());
    }

    public IEnumerable<string> GetFiles(string root, List<string> includes, List<string> excludes)
    {
        return Files.Where(x => x.Value != string.Empty &&
                                x.Key.StartsWith(GetFullPath(root)))
            .Select(x => x.Key).ToList();
    }

    public string GetFullPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path.ToInternal();
        }
        else
        {
            return Path.Combine(Root, path).ToInternal();
        }
    }

    public string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path);
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
        string ipath = GetFullPath(path).ToInternal();
        if (Files.TryGetValue(ipath, out var content) && !string.IsNullOrEmpty(content))
        {
            return content;
        }

        return string.Empty;
    }

    public string[] ReadAllLines(string path)
    {
        if (Files.TryGetValue(GetFullPath(path).ToInternal(), out var content) && !string.IsNullOrEmpty(content))
        {
            return content.Replace("'\r", string.Empty).Split('\n');
        }

        return [];
    }

    public void WriteAllText(string path, string content)
    {
        string ipath = GetFullPath(path).ToInternal();
        if (Files.TryGetValue(ipath, out var x))
        {
            Files.Remove(ipath);
        }
        Files.Add(ipath, content!);
    }

    public Stream OpenRead(string path)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(ReadAllText(path)));
    }

    public FolderData? GetFolderDataStructure()
    {
        return GetFolderData(null, Root);
    }

    private FolderData? GetFolderData(FolderData? parent, string dirpath)
    {
        // set basic folder information
        FolderData? folder = new()
        {
            Name = Path.GetFileName(dirpath),
            DisplayName = Path.GetFileNameWithoutExtension(dirpath),
            Path = dirpath,
            Parent = parent,
        };

        // add files in this folder
        var files = Files.Where(x => x.Value != string.Empty &&
                        x.Key.StartsWith(dirpath.ToInternal()) &&
                        !x.Key.Substring(Math.Min(dirpath.ToInternal().Length + 1, x.Key.Length)).Contains("/"));
        foreach (var file in files)
        {
            if (!file.Key.StartsWith("."))
            {
                folder.Files.Add(new()
                {
                    Parent = folder,
                    Name = Path.GetFileName(file.Key),
                    Path = Path.Combine(folder.Path, file.Key).ToInternal(),
                    DisplayName = Path.GetFileNameWithoutExtension(file.Key),
                });
            }
        }
        // order on sequence and display name
        folder.Files = folder.Files.OrderBy(x => x.Sequence).ThenBy(x => x.DisplayName).ToList();

        // add folders in this folder
        var subdirs = GetDirectories(dirpath.ToInternal());
        foreach (var subdir in subdirs)
        {
            var subfolder = GetFolderData(folder, subdir);
            if (subfolder != null)
            {
                folder.Folders.Add(subfolder);
            }
        }
        // order on sequence and display name
        folder.Folders = folder.Folders.OrderBy(x => x.Sequence).ThenBy(x => x.DisplayName).ToList();

        return folder;
    }

    public FolderData? FindFolderData(FolderData root, string path)
    {
        string fullPath = GetFullPath(path);
        string relPath = fullPath.Substring(root.Path.Length + 1);
        string[] subPaths = relPath.Split('/');

        FolderData? current = root;
        int i = 0;
        while (i < subPaths.Length)
        {
            current = current!.Folders.FirstOrDefault(x => x.Name.Equals(subPaths[i], StringComparison.OrdinalIgnoreCase));
            if (current == null)
            {
                // sub path not found.
                return null;
            }
            i++;
        }

        return current;
    }
}

