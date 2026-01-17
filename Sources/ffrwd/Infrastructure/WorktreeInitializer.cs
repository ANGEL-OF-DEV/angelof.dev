// WorktreeInitializer.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using LibGit2Sharp;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class WorktreeInitializer
{
  private const int MaxIndex = 99;

  public static WorktreeInitResult Initialize(
    Repository repo,
    string     model)
  {
    if (!TryResolveRoot(repo, out var repoRoot, out _))
    {
      return WorktreeInitResult.Fail("Error: repository has no working directory.",
                                     AgentExitCodes.RepoNotFound);
    }

    for (var index = 0; index <= MaxIndex; index++)
    {
      var identity     = IdentityFormat.Build(model, index);
      var worktreePath = GetWorktreePath(repoRoot, identity);

      if (PathExists(worktreePath))
      {
        using var existing = OpenWorktree(worktreePath);
        if (existing is null)
        {
          IssueLog.Record("Path exists but is not a git repository.");
          continue;
        }

        var branchName = existing.Head?.FriendlyName;
        if (!IdentityFormat.IsBranchForIdentity(branchName, identity))
        {
          IssueLog.Record("Path uses different identity.");
          continue;
        }

        var ensure = EnsureBranches(repo, identity);
        if (!ensure.Success)
        {
          return WorktreeInitResult.Fail(ensure.ErrorMessage,
                                         ensure.ExitCode);
        }

        return WorktreeInitResult.Ok(worktreePath);
      }

      var ensureNew = EnsureBranches(repo, identity);
      if (!ensureNew.Success)
      {
        return WorktreeInitResult.Fail(ensureNew.ErrorMessage,
                                       ensureNew.ExitCode);
      }

      var addResult = AddWorktree(repo,
                                  identity,
                                  worktreePath);
      if (!addResult.Success) { return addResult; }

      return WorktreeInitResult.Ok(worktreePath);
    }

    return WorktreeInitResult.Fail("Error: no available identity index.",
                                   AgentExitCodes.NoAvailableIdentity);
  }

  private static string GetWorktreePath(
    string repoRoot,
    string identity)
  {
    var path = $"{repoRoot}{IdentityFormat.WorktreeSuffix(identity)}";
    return Path.GetFullPath(path);
  }

  private static bool PathExists(string path)
  {
    return Directory.Exists(path) || File.Exists(path);
  }

  private static OperationResult EnsureBranches(
    Repository repo,
    string     identity)
  {
    var selfBranch = BranchNames.Self(identity);
    var workBranch = BranchNames.Work(identity);

    var selfOk = EnsureBranch(repo, selfBranch);
    if (!selfOk.Success) { return selfOk; }

    var workOk = EnsureBranch(repo, workBranch);
    if (!workOk.Success) { return workOk; }

    return OperationResult.Ok();
  }

  private static OperationResult EnsureBranch(
    Repository repo,
    string     branchName)
  {
    if (repo.Branches[branchName] is not null) { return OperationResult.Ok(); }

    try { repo.CreateBranch(branchName); }
    catch (LibGit2SharpException ex)
    {
      return OperationResult.Fail("Error: git branch failed.",
                                  AgentExitCodes.GitFailure,
                                  ex.Message);
    }

    return OperationResult.Ok();
  }

  private static WorktreeInitResult AddWorktree(
    Repository repo,
    string     identity,
    string     worktreePath)
  {
    var worktreeName  = identity;
    var branchExisted = repo.Branches[worktreeName] is not null;

    try
    {
      var worktree = repo.Worktrees.Add(BranchNames.Work(identity),
                                        worktreeName,
                                        worktreePath,
                                        false);
      if (worktree is null)
      {
        return WorktreeInitResult.Fail("Error: worktree add failed.",
                                       AgentExitCodes.GitFailure);
      }
    }
    catch (LibGit2SharpException ex)
    {
      return WorktreeInitResult.Fail("Error: worktree add failed.",
                                     AgentExitCodes.GitFailure,
                                     ex.Message);
    }

    if (!branchExisted) { TryRemoveBranch(repo, worktreeName); }

    return WorktreeInitResult.Ok(worktreePath);
  }

  private static void TryRemoveBranch(Repository repo, string branchName)
  {
    try
    {
      var branch = repo.Branches[branchName];
      if (branch is not null) { repo.Branches.Remove(branch); }
    }
    catch (LibGit2SharpException ex)
    {
      IssueLog.Record(string.Concat("Failed to remove branch: ", ex.Message));
    }
  }

  private static Repository? OpenWorktree(string path)
  {
    if (!Repository.IsValid(path)) { return null; }

    try { return new Repository(path); }
    catch (LibGit2SharpException ex)
    {
      IssueLog.Record(string.Concat("Failed to open worktree: ", ex.Message));
      return null;
    }
  }

  private static bool TryResolveRoot(
    Repository repo,
    out string repoRoot,
    out bool   isBare)
  {
    isBare = repo.Info.IsBare;
    var path = isBare
                 ? repo.Info.Path
                 : repo.Info.WorkingDirectory;

    if (string.IsNullOrWhiteSpace(path))
    {
      repoRoot = string.Empty;
      return false;
    }

    repoRoot = Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar,
                                             Path.AltDirectorySeparatorChar));
    return true;
  }
}
