// MutatingGuard.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class MutatingGuard
{
  public static WorktreeInfo? EnsurePrimaryRoot(string workingDirectory, IGitRunner git, List<string> errors)
  {
    var primary = WorktreeResolver.ResolvePrimary(workingDirectory, git, errors);
    if (primary is null)
      return null;

    var branch = GitHelpers.CurrentBranch(primary.Path, git, errors);
    if (branch is null)
      return null;

    if (!string.Equals(branch, "default", StringComparison.Ordinal))
    {
      errors.Add($"primary worktree must be on default (current: {branch})");
      return null;
    }

    if (!GitHelpers.IsClean(primary.Path, git, errors))
    {
      errors.Add("primary worktree must be clean before mutating PR state");
      return null;
    }

    return primary with { BranchName = branch };
  }
}
