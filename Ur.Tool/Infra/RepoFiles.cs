namespace Ur.Tool.Infra;

public static class RepoFiles
{
  public static IEnumerable<string> EnumerateFiles(string repoRoot, string searchPattern, bool recursive = true)
  {
    var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    return Directory.EnumerateFiles(repoRoot, searchPattern, opt);
  }

  public static string GetRepoRootOrCurrent()
  {
    // No git dependency: use explicit/sibling UR root when available.
    return UrRootResolver.Resolve(null);
  }
}
