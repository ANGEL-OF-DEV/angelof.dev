// CreateMetaLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.CreateMeta;

public static class CreateMetaLogic
{
  public static CommandResult Run(CreateMetaOptions options, CommandContext context)
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
    if (options.Children.Count == 0)
      errors.Add("--children is required");

    var status = NormalizeStatus(options.Status, errors);
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = MutatingGuard.EnsurePrimaryRoot(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    using var handle = LockManager.Acquire(primary.Path, "pr create-meta", errors);
    if (handle is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var realms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var children = new List<string>();

    foreach (var child in options.Children)
    {
      if (!child.StartsWith("pr://", StringComparison.OrdinalIgnoreCase))
      {
        errors.Add($"child must be pr:// URI: {child}");
        continue;
      }

      var childId = child.Substring("pr://".Length);
      var childPath = PrDocStore.FindById(primary.Path, childId, errors);
      if (childPath is null)
        continue;

      var childDoc = PrDocStore.Load(primary.Path, childPath, errors);
      if (childDoc is null)
        continue;

      foreach (var realm in childDoc.RealmsTouched)
        realms.Add(realm);

      children.Add(child);
    }

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var mergeOrder = MergeOrderHelper.Parse(options.MergeOrder, errors) ?? MergeOrderHelper.DefaultOrder();
    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var now = context.UtcNow().ToString("O");
    var doc = new PrDoc
    {
      SchemaVersion = "pull_request.v0",
      ContentVersion = "0.1.0",
      Id = options.Id,
      CanonicalUri = "pr://" + options.Id,
      Kind = "meta",
      Status = status,
      Title = options.Title,
      Summary = options.Summary,
      CreatedAtUtc = now,
      Author = AuthorResolver.ResolveAuthor(),
      Children = children,
      MergeOrder = mergeOrder,
      RealmsTouched = realms.OrderBy(r => r, StringComparer.OrdinalIgnoreCase).ToList(),
      TurnRefs = new List<string>(),
      Checks = new List<PrCheck>
      {
        new PrCheck { Name = "pr.create-meta", Result = "pending" }
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

    Log(context, "pr create-meta", primary.Path, decisions, edits, warnings, errors);
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
