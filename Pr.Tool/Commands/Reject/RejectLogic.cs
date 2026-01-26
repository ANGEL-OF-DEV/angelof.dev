// RejectLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Reject;

public static class RejectLogic
{
  public static CommandResult Run(RejectOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (string.IsNullOrWhiteSpace(options.PrRef))
      errors.Add("pr reference is required");
    if (string.IsNullOrWhiteSpace(options.Note))
      errors.Add("--note is required");

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr reject", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var repoRel = PrDocStore.ResolvePathFromInput(primary.Path, options.PrRef, errors);
    if (repoRel is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var doc = PrDocStore.Load(primary.Path, repoRel, errors);
    if (doc is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    doc.Status = "rejected";
    doc.Review.Notes.Add(options.Note);

    var rejectedRel = PrPaths.BuildRejectedRel(doc.Id);
    PrDocStore.Save(primary.Path, rejectedRel, doc);
    edits.Add($"wrote {rejectedRel}");

    var oldPath = RepoPath.ResolvePath(primary.Path, repoRel);
    File.Delete(oldPath);
    edits.Add($"removed {repoRel}");

    UpdateIndex(primary.Path, doc, context, edits, warnings, errors);

    Log(context, "pr reject", primary.Path, decisions, edits, warnings, errors);
    return errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);
  }

  private static void UpdateIndex(
    string repoRoot,
    PrDoc doc,
    CommandContext context,
    List<string> edits,
    List<string> warnings,
    List<string> errors)
  {
    var indexPath = RepoPath.ResolvePath(repoRoot, PrPaths.PendingIndexRel);
    if (!File.Exists(indexPath))
    {
      warnings.Add($"pending index not found: {PrPaths.PendingIndexRel}");
      return;
    }

    var index = PendingIndexStore.LoadOrCreate(repoRoot, context.UtcNow, errors);
    if (errors.Count > 0)
      return;

    index.UpdatedAtUtc = context.UtcNow().ToString("O");
    PendingIndexStore.Remove(index, doc.CanonicalUri);
    PendingIndexStore.Save(repoRoot, index);
    edits.Add($"updated {PrPaths.PendingIndexRel}");
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
