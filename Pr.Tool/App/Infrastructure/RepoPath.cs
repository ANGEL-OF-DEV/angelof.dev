// RepoPath.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class RepoPath
{
  public static bool IsRepoRelative(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return false;

    if (Path.IsPathRooted(path))
      return false;

    if (path.Contains(":", StringComparison.Ordinal))
      return false;

    if (path.Contains("..", StringComparison.Ordinal))
      return false;

    return true;
  }

  public static string ResolvePath(string repoRoot, string relPath)
  {
    var normalized = relPath.Replace('\\', '/');
    var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return Path.Combine(new[] { repoRoot }.Concat(parts).ToArray());
  }

  public static string ToRepoRelative(string repoRoot, string fullPath)
  {
    return Path.GetRelativePath(repoRoot, fullPath).Replace('\\', '/');
  }

  public static string NormalizeRepoRootDisplay(string? repoRootArg)
  {
    if (string.IsNullOrWhiteSpace(repoRootArg))
      return ".";

    return Path.IsPathRooted(repoRootArg) ? "." : repoRootArg;
  }
}
