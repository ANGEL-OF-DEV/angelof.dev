// AgentTaskingReader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class AgentTaskingReader
{
  public static bool TryLoad(
    string                 path,
    out AgentTaskingConfig config,
    out string?            errorMessage)
  {
    config       = new AgentTaskingConfig();
    errorMessage = null;

    if (!File.Exists(path))
    {
      errorMessage = $"Error: agent doctrine not found: {path}";
      return false;
    }

    if (!FrontmatterReader.TryRead(path, out var yaml, out errorMessage)) { return false; }

    try
    {
      var deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .Build();
      var result = deserializer.Deserialize<AgentTaskingConfig>(yaml);
      if (result is null)
      {
        errorMessage = "Error: agent doctrine frontmatter missing.";
        return false;
      }

      config = result;
      return true;
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to parse agent doctrine. {ex.Message}";
      return false;
    }
    catch (InvalidOperationException ex)
    {
      errorMessage = $"Error: failed to parse agent doctrine. {ex.Message}";
      return false;
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
      errorMessage = $"Error: failed to parse agent doctrine. {ex.Message}";
      return false;
    }
  }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class AgentTaskingConfig
{
  [YamlMember(Alias = "system_branch")]
  public SystemBranchConfig? SystemBranch { get; set; }

  [YamlMember(Alias = "task_sources")]
  public List<AgentTaskSourceConfig>? TaskSources { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class AgentTaskSourceConfig
{
  [YamlMember(Alias = "id")]
  public string? Id { get; set; }

  [YamlMember(Alias = "doctrine")]
  public string? Doctrine { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class SystemBranchConfig
{
  [YamlMember(Alias = "name")]
  public string? Name { get; set; }

  [YamlMember(Alias = "operator")]
  public string? Operator { get; set; }

  [YamlMember(Alias = "root")]
  public string? Root { get; set; }

  [YamlMember(Alias = "tasks_root")]
  public string? TasksRoot { get; set; }

  [YamlMember(Alias = "sequence_file")]
  public string? SequenceFile { get; set; }

  [YamlMember(Alias = "notes_path_template")]
  public string? NotesPathTemplate { get; set; }
}
