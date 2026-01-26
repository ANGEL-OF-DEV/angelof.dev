// ListLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.List;

public static class ListLogic
{
  public static ListResult Run(ListOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();
    var lines = new List<string>();

    var primary = WorktreeResolver.ResolvePrimary(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return new ListResult(CommandResult.Failure(errors, warnings, decisions, edits), lines);

    AppendDrafts(primary.Path, lines, errors);
    AppendPending(primary.Path, lines, errors);

    var result = errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);

    Log(context, "pr list", primary.Path, decisions, edits, warnings, errors);
    return new ListResult(result, lines);
  }

  private static void AppendDrafts(string repoRoot, List<string> lines, List<string> errors)
  {
    var draftDir = RepoPath.ResolvePath(repoRoot, PrPaths.DraftRel);
    if (!Directory.Exists(draftDir))
      return;

    foreach (var file in Directory.GetFiles(draftDir, "*.pr.yaml"))
    {
      var rel = RepoPath.ToRepoRelative(repoRoot, file);
      var doc = PrDocStore.Load(repoRoot, rel, errors);
      if (doc is null)
        continue;

      lines.Add($"draft {doc.CanonicalUri} - {doc.Title} ({PrDocStore.ToFileRepoUri(rel)})");
    }
  }

  private static void AppendPending(string repoRoot, List<string> lines, List<string> errors)
  {
    var indexPath = RepoPath.ResolvePath(repoRoot, PrPaths.PendingIndexRel);
    if (File.Exists(indexPath))
    {
      var text = File.ReadAllText(indexPath);
      var index = YamlHelpers.Deserialize<PendingIndex>(text, PrPaths.PendingIndexRel, errors);
      if (index is null)
        return;

      foreach (var entry in index.Pending)
        lines.Add($"pending {entry.PrUri} - {entry.Title} ({entry.PrFileRepoUri})");

      return;
    }

    var pendingDir = RepoPath.ResolvePath(repoRoot, PrPaths.PendingRel);
    if (!Directory.Exists(pendingDir))
      return;

    foreach (var file in Directory.GetFiles(pendingDir, "*.pr.yaml"))
    {
      var rel = RepoPath.ToRepoRelative(repoRoot, file);
      var doc = PrDocStore.Load(repoRoot, rel, errors);
      if (doc is null)
        continue;

      lines.Add($"pending {doc.CanonicalUri} - {doc.Title} ({PrDocStore.ToFileRepoUri(rel)})");
    }
  }

  private static void Log(
    CommandContext context,
    string command,
    string repoRoot,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits,
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> errors)
  {
    using var logger = LogWriter.Create(repoRoot, context.LogOptions, errors.ToList());
    logger?.Write(new LogEntry(
      DateTimeOffset.UtcNow.ToString("O"),
      command,
      RepoPath.NormalizeRepoRootDisplay(repoRoot),
      decisions,
      edits,
      warnings,
      errors.Count == 0 ? null : errors
    ));
  }
}
