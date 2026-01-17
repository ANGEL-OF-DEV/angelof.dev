// AgentTaskNextCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using angelof.dev.ffrwd.Infrastructure;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentTaskNextCommand : Command<AgentTaskNextSettings>
{
  private static readonly JsonSerializerOptions JsonOptionsPretty = new() { WriteIndented = true };

  private static readonly JsonSerializerOptions JsonOptionsCompact = new()
  {
    WriteIndented = false
  };

  public override int Execute(
    CommandContext        context,
    AgentTaskNextSettings settings,
    CancellationToken     cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return AgentExitCodes.RepoNotFound;
    }

    var protocolPath = ResolvePath(repoRoot, settings.Source);
    if (!DoctrineProtocolReader.TryLoad(protocolPath,
                                        out var doctrineConfig,
                                        out var doctrineError))
    {
      if (!string.IsNullOrWhiteSpace(doctrineError)) { Console.Error.WriteLine(doctrineError); }

      return AgentExitCodes.InvalidArguments;
    }

    if (string.IsNullOrWhiteSpace(doctrineConfig.AgentTasking?.Doctrine))
    {
      Console.Error.WriteLine("Error: agent tasking doctrine not set.");
      return AgentExitCodes.InvalidArguments;
    }

    var agentDoctrinePath = ResolvePath(repoRoot,
                                        doctrineConfig.AgentTasking.Doctrine);
    if (!AgentTaskingReader.TryLoad(agentDoctrinePath,
                                    out var agentConfig,
                                    out var agentError))
    {
      if (!string.IsNullOrWhiteSpace(agentError)) { Console.Error.WriteLine(agentError); }

      return AgentExitCodes.InvalidArguments;
    }

    var taskSource = SelectTaskSource(agentConfig, settings.TaskSourceId);
    if (taskSource is null)
    {
      Console.Error.WriteLine($"Error: task source not found: {settings.TaskSourceId}");
      return AgentExitCodes.InvalidArguments;
    }

    if (string.IsNullOrWhiteSpace(taskSource.Doctrine))
    {
      Console.Error.WriteLine($"Error: task source doctrine missing: {taskSource.Id}");
      return AgentExitCodes.InvalidArguments;
    }

    var taskDoctrinePath = ResolvePath(repoRoot, taskSource.Doctrine);
    if (!TaskSequenceReader.TryLoad(taskDoctrinePath,
                                    out var taskDoc,
                                    out var taskDocError))
    {
      if (!string.IsNullOrWhiteSpace(taskDocError)) { Console.Error.WriteLine(taskDocError); }

      return AgentExitCodes.InvalidArguments;
    }

    if (string.IsNullOrWhiteSpace(taskDoc.TaskSource?.Index))
    {
      Console.Error.WriteLine($"Error: task source index missing: {taskDoctrinePath}");
      return AgentExitCodes.InvalidArguments;
    }

    var indexPath = ResolvePath(repoRoot, taskDoc.TaskSource.Index);
    if (!TodoIndexReader.TryLoad(indexPath,
                                 out var entries,
                                 out var indexError))
    {
      if (!string.IsNullOrWhiteSpace(indexError)) { Console.Error.WriteLine(indexError); }

      return AgentExitCodes.InvalidArguments;
    }

    var nextEntry = entries.FirstOrDefault(entry => IsOpen(entry.Status));
    if (nextEntry is null)
    {
      Console.Error.WriteLine("Error: no open tasks available.");
      return AgentExitCodes.NoTasksAvailable;
    }

    if (taskDoc.TaskSequence is null)
    {
      Console.Error.WriteLine($"Error: task sequence missing: {taskDoctrinePath}");
      return AgentExitCodes.InvalidArguments;
    }

    if (!TaskSequenceReader.TryBuildOutput(taskDoc.TaskSequence,
                                           out var sequenceOutput,
                                           out var sequenceError))
    {
      if (!string.IsNullOrWhiteSpace(sequenceError)) { Console.Error.WriteLine(sequenceError); }

      return AgentExitCodes.InvalidArguments;
    }

    if (agentConfig.SystemBranch is null)
    {
      Console.Error.WriteLine("Error: system branch config missing.");
      return AgentExitCodes.InvalidArguments;
    }

    if (string.IsNullOrWhiteSpace(agentConfig.SystemBranch.SequenceFile))
    {
      Console.Error.WriteLine("Error: system sequence file missing.");
      return AgentExitCodes.InvalidArguments;
    }

    var sequencePath = ResolvePath(repoRoot,
                                   agentConfig.SystemBranch.SequenceFile);
    if (!TaskIdAllocator.TryReserve(sequencePath, out var taskId, out var idError))
    {
      if (!string.IsNullOrWhiteSpace(idError)) { Console.Error.WriteLine(idError); }

      return AgentExitCodes.InvalidArguments;
    }

    var notesPath = ResolveNotesPath(agentConfig.SystemBranch,
                                     taskId);
    if (!string.IsNullOrWhiteSpace(notesPath))
    {
      var notesDir = Path.GetDirectoryName(ResolvePath(repoRoot, notesPath));
      if (!string.IsNullOrWhiteSpace(notesDir))
      {
        try { Directory.CreateDirectory(notesDir); }
        catch (ArgumentException ex)
        {
          Console.Error.WriteLine($"Error: failed to create notes directory. {ex.Message}");
          return AgentExitCodes.InvalidArguments;
        }
        catch (IOException ex)
        {
          Console.Error.WriteLine($"Error: failed to create notes directory. {ex.Message}");
          return AgentExitCodes.InvalidArguments;
        }
        catch (NotSupportedException ex)
        {
          Console.Error.WriteLine($"Error: failed to create notes directory. {ex.Message}");
          return AgentExitCodes.InvalidArguments;
        }
        catch (UnauthorizedAccessException ex)
        {
          Console.Error.WriteLine($"Error: failed to create notes directory. {ex.Message}");
          return AgentExitCodes.InvalidArguments;
        }
        catch (System.Security.SecurityException ex)
        {
          Console.Error.WriteLine($"Error: failed to create notes directory. {ex.Message}");
          return AgentExitCodes.InvalidArguments;
        }
      }
    }

    var output = new AgentTaskOutput
    {
      TaskId    = taskId,
      NotesPath = notesPath ?? string.Empty,
      Source = new TaskSourceOutput
      {
        Type       = taskDoc.TaskSource.Id ?? settings.TaskSourceId,
        Index      = taskDoc.TaskSource.Index,
        EntryIndex = nextEntry.Index,
        Path       = nextEntry.Path,
        Title      = nextEntry.Title,
        Status     = nextEntry.Status,
        Owner      = nextEntry.Owner
      },
      Sequence = sequenceOutput
    };

    var json = SerializeJson(output, settings.Pretty);
    Console.Out.WriteLine(json);
    return AgentExitCodes.Success;
  }

  private static AgentTaskSourceConfig? SelectTaskSource(
    AgentTaskingConfig agentConfig,
    string             taskSourceId)
  {
    var sources = agentConfig.TaskSources;
    if (sources is null || sources.Count == 0) { return null; }

    return sources.FirstOrDefault(source =>
                                    string.Equals(source.Id,
                                                  taskSourceId,
                                                  StringComparison.OrdinalIgnoreCase));
  }

  private static string? ResolveNotesPath(
    SystemBranchConfig systemBranch,
    long               taskId)
  {
    var template = systemBranch.NotesPathTemplate;
    if (!string.IsNullOrWhiteSpace(template))
    {
      return template.Replace("<task_id>",
                              taskId.ToString(CultureInfo.InvariantCulture),
                              StringComparison.Ordinal);
    }

    if (string.IsNullOrWhiteSpace(systemBranch.TasksRoot)) { return null; }

    return Path.Combine(systemBranch.TasksRoot,
                        taskId.ToString(CultureInfo.InvariantCulture),
                        "notes.md");
  }

  private static bool IsOpen(string? status)
  {
    return string.Equals(status?.Trim(), "open", StringComparison.OrdinalIgnoreCase);
  }

  private static string SerializeJson(AgentTaskOutput output, bool pretty)
  {
    var options = pretty ? JsonOptionsPretty : JsonOptionsCompact;
    return JsonSerializer.Serialize(output, options);
  }

  private static string ResolvePath(string root, string path)
  {
    if (Path.IsPathRooted(path)) { return Path.GetFullPath(path); }

    return Path.GetFullPath(Path.Combine(root, path));
  }

  private sealed class AgentTaskOutput
  {
    [JsonPropertyName("task_id")]
    public long TaskId { get; init; }

    [JsonPropertyName("notes_path")]
    public string NotesPath { get; init; } = string.Empty;

    [JsonPropertyName("source")]
    public TaskSourceOutput Source { get; init; } = new();

    [JsonPropertyName("sequence")]
    public TaskSequenceOutput Sequence { get; init; } = new();
  }

  private sealed class TaskSourceOutput
  {
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("index")]
    public string Index { get; init; } = string.Empty;

    [JsonPropertyName("entry_index")]
    public int EntryIndex { get; init; }

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; init; } = string.Empty;
  }
}
