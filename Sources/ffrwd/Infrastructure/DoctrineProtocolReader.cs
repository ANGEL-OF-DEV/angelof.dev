// DoctrineProtocolReader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class DoctrineProtocolReader
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  public static bool TryLoad(
    string                     path,
    out DoctrineProtocolConfig config,
    out string?                errorMessage)
  {
    config       = new DoctrineProtocolConfig();
    errorMessage = null;

    if (!File.Exists(path))
    {
      errorMessage = $"Error: doctrine protocol not found: {path}";
      return false;
    }

    string content;
    try { content = File.ReadAllText(path); }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to read doctrine protocol. {ex.Message}";
      return false;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to read doctrine protocol. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to read doctrine protocol. {ex.Message}";
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to read doctrine protocol. {ex.Message}";
      return false;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to read doctrine protocol. {ex.Message}";
      return false;
    }

    try
    {
      var result = JsonSerializer.Deserialize<DoctrineProtocolConfig>(content,
        JsonOptions);
      if (result is null)
      {
        errorMessage = "Error: doctrine protocol JSON missing.";
        return false;
      }

      config = result;
      return true;
    }
    catch (JsonException ex)
    {
      errorMessage =
        $"Error: failed to parse doctrine protocol JSON. {ex.Message}";
      return false;
    }
    catch (ArgumentException ex)
    {
      errorMessage =
        $"Error: failed to load doctrine protocol JSON. {ex.Message}";
      return false;
    }
    catch (InvalidOperationException ex)
    {
      errorMessage =
        $"Error: failed to load doctrine protocol JSON. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage =
        $"Error: failed to load doctrine protocol JSON. {ex.Message}";
      return false;
    }
  }
}

internal sealed class DoctrineProtocolConfig
{
  [JsonPropertyName("doctrine_location")]
  [YamlMember(Alias = "doctrine_location")]
  public string? DoctrineLocation { get; set; }

  [JsonPropertyName("todo_tracking")]
  [YamlMember(Alias = "todo_tracking")]
  public DoctrineTodoTrackingConfig? TodoTracking { get; set; }

  [JsonPropertyName("tools_and_commands")]
  [YamlMember(Alias = "tools_and_commands")]
  public DoctrineToolsAndCommandsConfig? ToolsAndCommands { get; set; }

  [JsonPropertyName("agent_tasking")]
  [YamlMember(Alias = "agent_tasking")]
  public DoctrineAgentTaskingConfig? AgentTasking { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by JSON deserialization.")]
internal sealed class DoctrineTodoTrackingConfig
{
  [JsonPropertyName("index")]
  [YamlMember(Alias = "index")]
  public string? Index { get; set; }

  [JsonPropertyName("items_path")]
  [YamlMember(Alias = "items_path")]
  public string? ItemsPath { get; set; }

  [JsonPropertyName("item_extension")]
  [YamlMember(Alias = "item_extension")]
  public string? ItemExtension { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by JSON deserialization.")]
internal sealed class DoctrineToolsAndCommandsConfig
{
  [JsonPropertyName("index")]
  [YamlMember(Alias = "index")]
  public string? Index { get; set; }

  [JsonPropertyName("tools_path")]
  [YamlMember(Alias = "tools_path")]
  public string? ToolsPath { get; set; }

  [JsonPropertyName("tool_card_template")]
  [YamlMember(Alias = "tool_card_template")]
  public string? ToolCardTemplate { get; set; }

  [JsonPropertyName("issues_index")]
  [YamlMember(Alias = "issues_index")]
  public string? IssuesIndex { get; set; }

  [JsonPropertyName("issues_path")]
  [YamlMember(Alias = "issues_path")]
  public string? IssuesPath { get; set; }

  [JsonPropertyName("dynamic_records_root")]
  [YamlMember(Alias = "dynamic_records_root")]
  public string? DynamicRecordsRoot { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by JSON deserialization.")]
internal sealed class DoctrineAgentTaskingConfig
{
  [JsonPropertyName("doctrine")]
  [YamlMember(Alias = "doctrine")]
  public string? Doctrine { get; set; }

  [JsonPropertyName("command_source")]
  [YamlMember(Alias = "command_source")]
  public string? CommandSource { get; set; }
}
