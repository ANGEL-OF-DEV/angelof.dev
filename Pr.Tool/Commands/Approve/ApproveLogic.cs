// ApproveLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Approve;

public static class ApproveLogic
{
  public static CommandResult Run(ApproveOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (string.IsNullOrWhiteSpace(options.PrRef))
      errors.Add("pr reference is required");
    if (string.IsNullOrWhiteSpace(options.ApprovedBy))
      errors.Add("--approved-by is required");

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr approve", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var repoRel = PrDocStore.ResolvePathFromInput(primary.Path, options.PrRef, errors);
    if (repoRel is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var doc = PrDocStore.Load(primary.Path, repoRel, errors);
    if (doc is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    if (!string.Equals(doc.Status, "pending", StringComparison.OrdinalIgnoreCase))
      errors.Add($"PR must be pending to approve (current: {doc.Status})");

    PrDocVerifier.Verify(doc, primary.Path, repoRel, context.Git, allowMultiRealm: false, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    if (doc.Head is null)
    {
      errors.Add("PR is missing head branch");
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    if (!GitHelpers.Merge(primary.Path, context.Git, doc.Head.Branch, options.NoFastForward, errors))
      return CommandResult.Failure(errors, warnings, decisions, edits);

    doc.Status = "approved";
    doc.Review.Approvals.Add(new PrApproval
    {
      By = options.ApprovedBy,
      AtUtc = context.UtcNow().ToString("O")
    });

    var approvedRel = PrPaths.BuildApprovedRel(doc.Id);
    PrDocStore.Save(primary.Path, approvedRel, doc);
    edits.Add($"wrote {approvedRel}");

    var oldPath = RepoPath.ResolvePath(primary.Path, repoRel);
    File.Delete(oldPath);
    edits.Add($"removed {repoRel}");

    UpdateIndex(primary.Path, doc, context, edits, warnings, errors);

    if (options.DeleteBranch)
      GitHelpers.DeleteBranch(primary.Path, context.Git, doc.Head.Branch, errors);

    if (options.PruneWorktree)
      PruneWorktree(primary.Path, context.Git, doc.Head.Branch, warnings, errors);

    Log(context, "pr approve", primary.Path, decisions, edits, warnings, errors);
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

  private static void PruneWorktree(string repoRoot, IGitRunner git, string branch, List<string> warnings, List<string> errors)
  {
    var path = WorktreeLookup.FindWorktreePathForBranch(repoRoot, git, branch, errors);
    if (path is null)
      return;

    var allowedRoot = RepoPath.ResolvePath(repoRoot, "[monocoque.dev-branches]");
    if (!path.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
    {
      warnings.Add($"worktree not under [monocoque.dev-branches]: {path}");
      return;
    }

    GitHelpers.RemoveWorktree(repoRoot, git, path, errors);
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
