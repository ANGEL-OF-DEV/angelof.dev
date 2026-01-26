// GitHelpers.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class GitHelpers
{
  public static string? CurrentBranch(string workingDirectory, IGitRunner git, List<string> errors)
  {
    var result = git.Run(workingDirectory, "branch", "--show-current");
    if (!result.Ok)
    {
      errors.Add($"git branch --show-current failed: {result.StdErr}".TrimEnd());
      return null;
    }

    var branch = result.StdOut.Trim();
    if (string.IsNullOrWhiteSpace(branch))
    {
      errors.Add("git branch --show-current returned empty branch");
      return null;
    }

    return branch;
  }

  public static bool IsClean(string workingDirectory, IGitRunner git, List<string> errors)
  {
    var result = git.Run(workingDirectory, "status", "--porcelain");
    if (!result.Ok)
    {
      errors.Add($"git status --porcelain failed: {result.StdErr}".TrimEnd());
      return false;
    }

    return string.IsNullOrWhiteSpace(result.StdOut);
  }

  public static bool BranchExists(string repoRoot, IGitRunner git, string branch, List<string> errors)
  {
    var result = git.Run(repoRoot, "show-ref", "--verify", $"refs/heads/{branch}");
    if (result.Ok)
      return true;

    errors.Add($"branch not found: {branch}");
    return false;
  }

  public static List<string> DiffNameOnly(string repoRoot, IGitRunner git, string baseBranch, string headBranch, List<string> errors)
  {
    var result = git.Run(repoRoot, "diff", "--name-only", $"{baseBranch}...{headBranch}");
    if (!result.Ok)
    {
      errors.Add($"git diff --name-only {baseBranch}...{headBranch} failed: {result.StdErr}".TrimEnd());
      return new List<string>();
    }

    return result.StdOut
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Select(line => line.Trim())
      .Where(line => !string.IsNullOrWhiteSpace(line))
      .ToList();
  }

  public static bool Merge(string repoRoot, IGitRunner git, string branch, bool noFastForward, List<string> errors)
  {
    var args = new List<string> { "merge" };
    if (noFastForward)
      args.Add("--no-ff");
    args.Add(branch);

    var result = git.Run(repoRoot, args);
    if (result.Ok)
      return true;

    errors.Add($"git merge failed: {result.StdErr}".TrimEnd());
    return false;
  }

  public static bool DeleteBranch(string repoRoot, IGitRunner git, string branch, List<string> errors)
  {
    var result = git.Run(repoRoot, "branch", "-d", branch);
    if (result.Ok)
      return true;

    errors.Add($"git branch -d failed: {result.StdErr}".TrimEnd());
    return false;
  }

  public static bool RemoveWorktree(string repoRoot, IGitRunner git, string worktreePath, List<string> errors)
  {
    var result = git.Run(repoRoot, "worktree", "remove", worktreePath);
    if (result.Ok)
      return true;

    errors.Add($"git worktree remove failed: {result.StdErr}".TrimEnd());
    return false;
  }
}
