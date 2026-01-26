// LogWriter.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;
using Pr.Tool.App.Infrastructure;

namespace Pr.Tool.App.Logging;

public sealed record LogOptions(string? LogSink, string? LogFile);

public sealed record LogEntry(
  string TimestampUtc,
  string Command,
  string RepoRoot,
  IReadOnlyList<string> Decisions,
  IReadOnlyList<string> Edits,
  IReadOnlyList<string> Warnings,
  IReadOnlyList<string>? Errors);

public sealed class LogWriter : IDisposable
{
  private readonly StreamWriter? _writer;
  private readonly bool _stdout;

  private LogWriter(StreamWriter? writer, bool stdout)
  {
    _writer = writer;
    _stdout = stdout;
  }

  public static LogWriter? Create(string repoRoot, LogOptions options, List<string> errors)
  {
    var sink = options.LogSink ?? string.Empty;
    var logToStdout = string.Equals(sink, "stdout", StringComparison.OrdinalIgnoreCase);
    if (logToStdout)
      return new LogWriter(null, true);

    var logFileRel = string.IsNullOrWhiteSpace(options.LogFile)
      ? "[logs.local]/pr.tool/" + DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + ".jsonl"
      : options.LogFile;

    if (!RepoPath.IsRepoRelative(logFileRel))
    {
      errors.Add($"log-file must be repo-relative: {logFileRel}");
      return null;
    }

    var path = RepoPath.ResolvePath(repoRoot, logFileRel);
    var dir = Path.GetDirectoryName(path);
    if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
      Directory.CreateDirectory(dir);

    var writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read));
    return new LogWriter(writer, false);
  }

  public void Write(LogEntry entry)
  {
    var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = false });
    if (_stdout)
    {
      Console.WriteLine(json);
      return;
    }

    _writer?.WriteLine(json);
    _writer?.Flush();
  }

  public void Dispose()
  {
    _writer?.Dispose();
  }
}
