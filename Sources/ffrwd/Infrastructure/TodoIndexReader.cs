// TodoIndexReader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class TodoIndexReader
{
  private static readonly string[] HeaderSplitter = new[] { ": " };

  public static bool TryLoad(
    string                   path,
    out List<TodoIndexEntry> entries,
    out string?              errorMessage)
  {
    entries      = new List<TodoIndexEntry>();
    errorMessage = null;

    if (!File.Exists(path))
    {
      errorMessage = $"Error: todo index not found: {path}";
      return false;
    }

    string[] lines;
    try { lines = File.ReadAllLines(path); }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to read todo index. {ex.Message}";
      return false;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to read todo index. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to read todo index. {ex.Message}";
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to read todo index. {ex.Message}";
      return false;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to read todo index. {ex.Message}";
      return false;
    }

    var startIndex = FindBodyStart(lines);
    var entryIndex = 0;
    for (var i = startIndex; i < lines.Length; i += 1)
    {
      var line = lines[i].Trim();
      if (line.Length < 2
       || line[0]     != '-'
       || line[1]     != ' ') { continue; }

      if (!TryParseEntry(line, entryIndex, out var entry)) { continue; }

      entries.Add(entry);
      entryIndex += 1;
    }

    return true;
  }

  private static int FindBodyStart(string[] lines)
  {
    if (lines.Length == 0) { return 0; }

    var first = NormalizeLine(lines[0]);
    if (first != "---") { return 0; }

    for (var i = 1; i < lines.Length; i += 1)
    {
      if (NormalizeLine(lines[i]) == "---") { return i + 1; }
    }

    return lines.Length;
  }

  private static bool TryParseEntry(
    string             line,
    int                entryIndex,
    out TodoIndexEntry entry)
  {
    entry = new TodoIndexEntry();
    var body     = line[2..].Trim();
    var segments = body.Split('|');
    if (segments.Length == 0) { return false; }

    var header = segments[0].Trim();
    var title  = header;
    var headerParts = header.Split(HeaderSplitter,
                                   2,
                                   StringSplitOptions.None);
    if (headerParts.Length == 2) { title = headerParts[1].Trim(); }

    var status = string.Empty;
    var owner  = string.Empty;
    var path   = string.Empty;
    for (var i = 1; i < segments.Length; i += 1)
    {
      var segment = segments[i].Trim();
      if (segment.StartsWith("status:", StringComparison.OrdinalIgnoreCase))
      {
        status = segment["status:".Length..].Trim();
        continue;
      }

      if (segment.StartsWith("owner:", StringComparison.OrdinalIgnoreCase))
      {
        owner = segment["owner:".Length..].Trim();
        continue;
      }

      if (segment.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
      {
        path = segment["path:".Length..].Trim();
        path = TrimBackticks(path);
      }
    }

    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(path)) { return false; }

    if (ContainsPlaceholder(title)
     || ContainsPlaceholder(status)
     || ContainsPlaceholder(owner)
     || ContainsPlaceholder(path)) { return false; }

    entry = new TodoIndexEntry
    {
      Index  = entryIndex,
      Title  = title,
      Status = status,
      Owner  = owner,
      Path   = path
    };
    return true;
  }

  private static string TrimBackticks(string value)
  {
    var trimmed = value.Trim();
    if (trimmed.Length >= 2
     && trimmed[0]     == '`'
     && trimmed[^1]    == '`') { return trimmed[1..^1]; }

    return trimmed;
  }

  private static bool ContainsPlaceholder(string value)
  {
    return value.Contains('<', StringComparison.Ordinal)
        || value.Contains('>', StringComparison.Ordinal);
  }

  private static string NormalizeLine(string line)
  {
    if (line.Length > 0 && line[0] == '\uFEFF') { return line[1..].Trim(); }

    return line.Trim();
  }
}

internal sealed class TodoIndexEntry
{
  public int    Index  { get; init; }
  public string Title  { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public string Owner  { get; init; } = string.Empty;
  public string Path   { get; init; } = string.Empty;
}
