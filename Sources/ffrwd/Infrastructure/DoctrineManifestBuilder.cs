// DoctrineManifestBuilder.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class DoctrineManifestBuilder
{
  public static Dictionary<string, object?> Build(
    DoctrineProtocolConfig config,
    string                 source)
  {
    var files        = new List<string>();
    var directories  = new List<string>();
    var fileSet      = new HashSet<string>(StringComparer.Ordinal);
    var directorySet = new HashSet<string>(StringComparer.Ordinal);

    AddUnique(files,       fileSet,      NormalizeFileReference(source));
    AddUnique(directories, directorySet, config.DoctrineLocation);

    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.AgentTasking?.Doctrine));
    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.ToolsAndCommands?.Index));
    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.AgentTasking?.CommandSource));
    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.ToolsAndCommands?.ToolCardTemplate));
    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.ToolsAndCommands?.IssuesIndex));
    AddUnique(files,
              fileSet,
              NormalizeFileReference(config.TodoTracking?.Index));

    AddUnique(directories, directorySet, config.TodoTracking?.ItemsPath);
    AddUnique(directories, directorySet, config.ToolsAndCommands?.ToolsPath);
    AddUnique(directories, directorySet, config.ToolsAndCommands?.IssuesPath);
    AddUnique(directories, directorySet, config.ToolsAndCommands?.DynamicRecordsRoot);

    return new Dictionary<string, object?>
    {
      ["source"] = source, ["doctrine_files"] = files, ["doctrine_directories"] = directories
    };
  }

  private static void AddUnique(
    List<string>    list,
    HashSet<string> seen,
    string?         value)
  {
    if (string.IsNullOrWhiteSpace(value)) { return; }

    var trimmed = value.Trim();
    if (seen.Add(trimmed)) { list.Add(trimmed); }
  }

  private static string? NormalizeFileReference(string? value)
  {
    if (string.IsNullOrWhiteSpace(value)) { return null; }

    var trimmed = value.Trim();
    if (trimmed.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) { return trimmed; }

    if (trimmed.EndsWith(".yml.md", StringComparison.OrdinalIgnoreCase))
    {
      return trimmed + ".json";
    }

    return trimmed;
  }
}
