// SubmitLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Submit;

public static class SubmitLogic
{
  public static CommandResult Run(SubmitOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (string.IsNullOrWhiteSpace(options.Id))
      errors.Add("--id is required");

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr submit", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var draftRel = PrPaths.BuildDraftRel(options.Id);
    var draftFull = RepoPath.ResolvePath(primary.Path, draftRel);
    if (!File.Exists(draftFull))
    {
      errors.Add($"draft PR not found: {draftRel}");
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    var doc = PrDocStore.Load(primary.Path, draftRel, errors);
    if (doc is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    PrDocVerifier.Verify(doc, primary.Path, draftRel, context.Git, allowMultiRealm: true, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    doc.Status = "pending";

    var pendingRel = PrPaths.BuildPendingRel(options.Id);
    PrDocStore.Save(primary.Path, pendingRel, doc);
    edits.Add($"wrote {pendingRel}");

    File.Delete(draftFull);
    edits.Add($"removed {draftRel}");

    var index = PendingIndexStore.LoadOrCreate(primary.Path, context.UtcNow, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    index.UpdatedAtUtc = context.UtcNow().ToString("O");
    PendingIndexStore.Upsert(index, new PendingEntry
    {
      PrUri = doc.CanonicalUri,
      PrFileRepoUri = PrDocStore.ToFileRepoUri(pendingRel),
      Title = doc.Title,
      Kind = doc.Kind,
      CreatedAtUtc = doc.CreatedAtUtc,
      RealmsTouched = doc.RealmsTouched
    });

    PendingIndexStore.Save(primary.Path, index);
    edits.Add($"updated {PrPaths.PendingIndexRel}");

    Log(context, "pr submit", primary.Path, decisions, edits, warnings, errors);
    return errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);
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
