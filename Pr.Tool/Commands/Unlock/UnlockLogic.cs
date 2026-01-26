// UnlockLogic.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Unlock;

public static class UnlockLogic
{
  public static CommandResult Run(UnlockOptions options, CommandContext context)
  {
    var errors = new List<string>();
    var warnings = new List<string>();
    var decisions = new List<string>();
    var edits = new List<string>();

    if (!options.Force)
      errors.Add("--force is required to remove lock");

    if (errors.Count > 0)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var primary = WorktreeResolver.ResolvePrimary(context.WorkingDirectory, context.Git, errors);
    if (primary is null)
      return CommandResult.Failure(errors, warnings, decisions, edits);

    var lockPath = LockManager.ResolveLockPath(primary.Path);
    if (!File.Exists(lockPath))
    {
      errors.Add($"lock file not found: {lockPath}");
      return CommandResult.Failure(errors, warnings, decisions, edits);
    }

    var content = File.ReadAllText(lockPath);
    Console.WriteLine(content);

    File.Delete(lockPath);
    edits.Add($"removed {RepoPath.ToRepoRelative(primary.Path, lockPath)}");

    Log(context, "pr unlock", primary.Path, decisions, edits, warnings, errors);
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
