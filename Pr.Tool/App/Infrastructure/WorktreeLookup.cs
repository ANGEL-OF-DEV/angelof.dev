// WorktreeLookup.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class WorktreeLookup
{
  public static string? FindWorktreePathForBranch(string repoRoot, IGitRunner git, string branch, List<string> errors)
  {
    var result = git.Run(repoRoot, "worktree", "list", "--porcelain");
    if (!result.Ok)
    {
      errors.Add($"git worktree list --porcelain failed: {result.StdErr}".TrimEnd());
      return null;
    }

    var entries = WorktreeResolver.Parse(result.StdOut, errors);
    if (errors.Count > 0)
      return null;

    var targetRef = "refs/heads/" + branch;
    foreach (var entry in entries)
    {
      if (string.Equals(entry.BranchRef, targetRef, StringComparison.Ordinal))
        return entry.Path;
    }

    return null;
  }
}
