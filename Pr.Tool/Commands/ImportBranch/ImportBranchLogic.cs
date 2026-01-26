// ImportBranchLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.ImportBranch;

public static class ImportBranchLogic
{
  public static CommandResult Run(ImportBranchOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (string.IsNullOrWhiteSpace(options.HeadBranch))
      errors.Add("--head is required");

    var status = NormalizeStatus(options.Status, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr import-branch", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var id = DeriveIdFromBranch(options.HeadBranch);
    var realms = RealmInference.InferFromDiff(primary.Path, context.Git, options.BaseBranch, options.HeadBranch, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var turnRefs = NormalizeTurnRefs(options.TurnRefs, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    if (options.GuessTurns)
    {
      var guessed = TurnRefGuesser.GuessTurnRefs(primary.Path, options.HeadBranch);
      foreach (var guess in guessed)
      {
        if (!turnRefs.Contains(guess, StringComparer.OrdinalIgnoreCase))
          turnRefs.Add(guess);
      }
    }

    var now = context.UtcNow().ToString("O");
    var doc = new PrDoc
    {
      SchemaVersion = "pull_request.v0",
      ContentVersion = "0.1.0",
      Id = id,
      CanonicalUri = "pr://" + id,
      Kind = "atomic",
      Status = status,
      Title = $"Import {options.HeadBranch}",
      Summary = $"Imported from branch {options.HeadBranch}",
      CreatedAtUtc = now,
      Author = AuthorResolver.ResolveAuthor(),
      Base = new PrBranch { Branch = options.BaseBranch },
      Head = new PrBranch { Branch = options.HeadBranch },
      Children = new List<string>(),
      MergeOrder = new List<string>(),
      RealmsTouched = realms,
      TurnRefs = turnRefs,
      Checks = new List<PrCheck>
      {
        new PrCheck { Name = "pr.import-branch", Result = "pending" }
      },
      Review = new PrReview()
    };

    if (turnRefs.Count == 0)
      doc.Review.Notes.Add("TURN_REFS_MISSING");

    var repoRel = status == "pending"
      ? PrPaths.BuildPendingRel(id)
      : PrPaths.BuildDraftRel(id);

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

    Log(context, "pr import-branch", primary.Path, decisions, edits, warnings, errors);
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

  private static string DeriveIdFromBranch(string branch)
  {
    var normalized = branch.Trim();
    var cleaned = normalized.Replace('/', '-').Replace('\\', '-');
    return cleaned;
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
