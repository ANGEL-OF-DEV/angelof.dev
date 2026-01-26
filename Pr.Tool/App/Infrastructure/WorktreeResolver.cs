// WorktreeResolver.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public sealed record WorktreeInfo(string Path, string? BranchName, string? BranchRef);

public static class WorktreeResolver
{
  public static WorktreeInfo? ResolvePrimary(string workingDirectory, IGitRunner git, List<string> errors)
  {
    var result = git.Run(workingDirectory, "worktree", "list", "--porcelain");
    if (!result.Ok)
    {
      errors.Add($"git worktree list --porcelain failed: {result.StdErr}".TrimEnd());
      return null;
    }

    var entries = Parse(result.StdOut, errors);
    if (errors.Count > 0)
      return null;

    if (entries.Count == 0)
    {
      errors.Add("git worktree list --porcelain: no worktrees found");
      return null;
    }

    return entries[0];
  }

  public static List<WorktreeInfo> Parse(string output, List<string> errors)
  {
    var entries = new List<WorktreeInfo>();
    string? path = null;
    string? branchRef = null;

    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    foreach (var raw in lines)
    {
      var line = raw.Trim();
      if (line.StartsWith("worktree ", StringComparison.Ordinal))
      {
        if (path is not null)
        {
          entries.Add(new WorktreeInfo(path, ExtractBranch(branchRef), branchRef));
          branchRef = null;
        }

        path = line.Substring("worktree ".Length).Trim();
        continue;
      }

      if (line.StartsWith("branch ", StringComparison.Ordinal))
      {
        branchRef = line.Substring("branch ".Length).Trim();
      }
    }

    if (path is not null)
      entries.Add(new WorktreeInfo(path, ExtractBranch(branchRef), branchRef));

    if (entries.Count == 0)
      errors.Add("git worktree list --porcelain: unable to parse worktree output");

    return entries;
  }

  private static string? ExtractBranch(string? branchRef)
  {
    if (string.IsNullOrWhiteSpace(branchRef))
      return null;

    const string prefix = "refs/heads/";
    if (branchRef.StartsWith(prefix, StringComparison.Ordinal))
      return branchRef.Substring(prefix.Length);

    return branchRef;
  }
}
