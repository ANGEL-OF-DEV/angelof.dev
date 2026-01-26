// CreateLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Create;

public static class CreateLogic
{
  public static CommandResult Run(CreateOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (string.IsNullOrWhiteSpace(options.Id))
      errors.Add("--id is required");
    if (string.IsNullOrWhiteSpace(options.Title))
      errors.Add("--title is required");
    if (string.IsNullOrWhiteSpace(options.Summary))
      errors.Add("--summary is required");

    var status = NormalizeStatus(options.Status, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr create", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var headBranch = options.HeadBranch;
    if (string.IsNullOrWhiteSpace(headBranch))
      headBranch = GitHelpers.CurrentBranch(context.WorkingDirectory, context.Git, errors);

    if (string.IsNullOrWhiteSpace(headBranch))
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var realms = options.Realms.Count > 0
      ? NormalizeRealms(options.Realms, errors)
      : RealmInference.InferFromDiff(primary.Path, context.Git, options.BaseBranch, headBranch, errors);

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var turnRefs = NormalizeTurnRefs(options.TurnRefs, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var now = context.UtcNow().ToString("O");
    var doc = new PrDoc
    {
      SchemaVersion = "pull_request.v0",
      ContentVersion = "0.1.0",
      Id = options.Id,
      CanonicalUri = "pr://" + options.Id,
      Kind = "atomic",
      Status = status,
      Title = options.Title,
      Summary = options.Summary,
      CreatedAtUtc = now,
      Author = AuthorResolver.ResolveAuthor(),
      Base = new PrBranch { Branch = options.BaseBranch },
      Head = new PrBranch { Branch = headBranch },
      Children = new List<string>(),
      MergeOrder = new List<string>(),
      RealmsTouched = realms,
      TurnRefs = turnRefs,
      Checks = new List<PrCheck>
      {
        new PrCheck { Name = "pr.create", Result = "pending" }
      },
      Review = new PrReview()
    };

    var repoRel = status == "pending"
      ? PrPaths.BuildPendingRel(options.Id)
      : PrPaths.BuildDraftRel(options.Id);

    PrDocStore.Save(primary.Path, repoRel, doc);
    edits.Add($"wrote {repoRel}");

    if (status == "pending")
    {
      var index = PendingIndexStore.LoadOrCreate(primary.Path, context.UtcNow, errors);
      if (errors.Count > 0)
        return CommandResult.Failure(errors, warnings, decisions, edits);

      index.UpdatedAtUtc = now;
      PendingIndexStore.Upsert(index, new PendingEntry
      {
        PrUri = doc.CanonicalUri,
        PrFileRepoUri = PrDocStore.ToFileRepoUri(repoRel),
        Title = doc.Title,
        Kind = doc.Kind,
        CreatedAtUtc = doc.CreatedAtUtc,
        RealmsTouched = doc.RealmsTouched
      });

      PendingIndexStore.Save(primary.Path, index);
      edits.Add($"updated {PrPaths.PendingIndexRel}");
    }

    Log(context, "pr create", primary.Path, decisions, edits, warnings, errors);
    return errors.Count == 0
      ? CommandResult.Success(warnings, decisions, edits)
      : CommandResult.Failure(errors, warnings, decisions, edits);
  }

  private static string NormalizeStatus(string status, List<string> errors)
  {
    if (string.IsNullOrWhiteSpace(status))
      return "draft";

    var normalized = status.Trim().ToLowerInvariant();
    return normalized switch
    {
      "draft" => "draft",
      "pending" => "pending",
      _ => FailStatus(status, errors)
    };
  }

  private static string FailStatus(string status, List<string> errors)
  {
    errors.Add($"invalid status: {status}");
    return "draft";
  }

  private static List<string> NormalizeRealms(IReadOnlyList<string> realms, List<string> errors)
  {
    var list = new List<string>();
    foreach (var realm in realms)
    {
      if (string.IsNullOrWhiteSpace(realm))
        continue;

      if (!realm.StartsWith("realm://", StringComparison.OrdinalIgnoreCase))
      {
        errors.Add($"realm must be realm://...: {realm}");
        continue;
      }

      list.Add(realm);
    }

    if (list.Count == 0)
      errors.Add("at least one realm is required");

    return list.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(r => r, StringComparer.OrdinalIgnoreCase).ToList();
  }

  private static List<string> NormalizeTurnRefs(IReadOnlyList<string> refs, List<string> errors)
  {
    var list = new List<string>();
    foreach (var turn in refs)
    {
      if (string.IsNullOrWhiteSpace(turn))
        continue;

      if (!FileRepoUri.IsFileRepoUri(turn))
      {
        errors.Add($"turn ref must be file.repo://...: {turn}");
        continue;
      }

      list.Add(turn);
    }

    return list;
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
