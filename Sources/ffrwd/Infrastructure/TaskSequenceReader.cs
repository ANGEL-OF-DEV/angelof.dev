// TaskSequenceReader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class TaskSequenceReader
{
  public static bool TryLoad(
    string                   path,
    out TaskSequenceDocument doc,
    out string?              errorMessage)
  {
    doc          = new TaskSequenceDocument();
    errorMessage = null;

    if (!File.Exists(path))
    {
      errorMessage = $"Error: task sequence doc not found: {path}";
      return false;
    }

    if (!FrontmatterReader.TryRead(path, out var yaml, out errorMessage)) { return false; }

    try
    {
      var deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .Build();
      var result = deserializer.Deserialize<TaskSequenceDocument>(yaml);
      if (result is null)
      {
        errorMessage = "Error: task sequence frontmatter missing.";
        return false;
      }

      doc = result;
      return true;
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to parse task sequence. {ex.Message}";
      return false;
    }
    catch (InvalidOperationException ex)
    {
      errorMessage = $"Error: failed to parse task sequence. {ex.Message}";
      return false;
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
      errorMessage = $"Error: failed to parse task sequence. {ex.Message}";
      return false;
    }
  }

  public static bool TryBuildOutput(
    TaskSequenceConfig     config,
    out TaskSequenceOutput output,
    out string?            errorMessage)
  {
    output       = new TaskSequenceOutput();
    errorMessage = null;

    var steps = config.Steps;
    if (steps is null || steps.Count == 0)
    {
      errorMessage = "Error: task sequence steps missing.";
      return false;
    }

    var displayMode = NormalizeMode(config.Display?.Mode);
    if (string.IsNullOrWhiteSpace(displayMode)) { displayMode = "SEQUENTIAL"; }

    List<string> displayIds;
    string?      displayPrefix = null;
    if (displayMode == "EXPLICIT")
    {
      displayIds = config.Display?.Ids ?? new List<string>();
      if (displayIds.Count != steps.Count)
      {
        errorMessage = "Error: display ids count mismatch.";
        return false;
      }
    }
    else if (displayMode == "SEQUENTIAL")
    {
      var prefix = config.Display?.Prefix;
      if (string.IsNullOrWhiteSpace(prefix)) { prefix = "T"; }
      else { prefix                                   = prefix.Trim(); }

      displayPrefix = prefix;
      displayIds    = new List<string>(steps.Count);
      for (var i = 0; i < steps.Count; i += 1) { displayIds.Add($"{prefix}{i + 1}"); }
    }
    else
    {
      errorMessage = $"Error: unsupported display mode: {displayMode}";
      return false;
    }

    var outputSteps = new List<TaskSequenceStepOutput>(steps.Count);
    for (var i = 0; i < steps.Count; i += 1)
    {
      var step = steps[i];
      if (string.IsNullOrWhiteSpace(step.Title))
      {
        errorMessage = "Error: task sequence step title missing.";
        return false;
      }

      outputSteps.Add(new TaskSequenceStepOutput
      {
        DisplayId   = displayIds[i],
        Title       = step.Title,
        Description = step.Description ?? string.Empty
      });
    }

    output = new TaskSequenceOutput
    {
      Id            = config.Id ?? string.Empty,
      DisplayMode   = displayMode,
      DisplayPrefix = displayPrefix,
      Steps         = outputSteps
    };

    return true;
  }

  private static string? NormalizeMode(string? mode)
  {
    if (string.IsNullOrWhiteSpace(mode)) { return null; }

    return mode.Trim().ToUpperInvariant();
  }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class TaskSequenceDocument
{
  [YamlMember(Alias = "task_source")]
  public TaskSourceConfig? TaskSource { get; set; }

  [YamlMember(Alias = "task_sequence")]
  public TaskSequenceConfig? TaskSequence { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class TaskSourceConfig
{
  [YamlMember(Alias = "id")]
  public string? Id { get; set; }

  [YamlMember(Alias = "index")]
  public string? Index { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class TaskSequenceConfig
{
  [YamlMember(Alias = "id")]
  public string? Id { get; set; }

  [YamlMember(Alias = "display")]
  public TaskSequenceDisplayConfig? Display { get; set; }

  [YamlMember(Alias = "steps")]
  public List<TaskSequenceStepConfig>? Steps { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class TaskSequenceDisplayConfig
{
  [YamlMember(Alias = "mode")]
  public string? Mode { get; set; }

  [YamlMember(Alias = "prefix")]
  public string? Prefix { get; set; }

  [YamlMember(Alias = "ids")]
  public List<string>? Ids { get; set; }
}

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Used by YAML deserialization.")]
internal sealed class TaskSequenceStepConfig
{
  [YamlMember(Alias = "title")]
  public string? Title { get; set; }

  [YamlMember(Alias = "description")]
  public string? Description { get; set; }
}

internal sealed class TaskSequenceOutput
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("display_mode")]
  public string DisplayMode { get; init; } = string.Empty;

  [JsonPropertyName("display_prefix")]
  public string? DisplayPrefix { get; init; }

  [JsonPropertyName("steps")]
  public IReadOnlyList<TaskSequenceStepOutput> Steps { get; init; }
    = Array.Empty<TaskSequenceStepOutput>();
}

internal sealed class TaskSequenceStepOutput
{
  [JsonPropertyName("display_id")]
  public string DisplayId { get; init; } = string.Empty;

  [JsonPropertyName("title")]
  public string Title { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string Description { get; init; } = string.Empty;
}
