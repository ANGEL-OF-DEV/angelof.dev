// RepositoryLocator.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using LibGit2Sharp;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class RepositoryLocator
{
  public static string? FindRoot(string? startPath = null)
  {
    var start   = startPath ?? Directory.GetCurrentDirectory();
    var gitPath = Repository.Discover(start);
    if (!string.IsNullOrWhiteSpace(gitPath))
    {
      try
      {
        using var repo = new Repository(gitPath);
        if (TryNormalize(repo.Info.WorkingDirectory, out var root)) { return root; }

        if (repo.Info.IsBare
         && TryNormalize(repo.Info.Path, out var bareRoot)) { return bareRoot; }

        if (!repo.Info.IsBare
         && TryNormalize(Path.GetDirectoryName(gitPath), out var fallback)) { return fallback; }
      }
      catch (LibGit2SharpException)
      {
        // Fall through to marker-based discovery.
      }
    }

    return FindRootFromMarkers(start);
  }

  private static bool TryNormalize(string? path, out string normalized)
  {
    normalized = string.Empty;
    if (string.IsNullOrWhiteSpace(path)) { return false; }

    normalized = Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar,
                                               Path.AltDirectorySeparatorChar));
    return true;
  }

  private static string? FindRootFromMarkers(string startPath)
  {
    var current = new DirectoryInfo(startPath);
    while (current is not null)
    {
      var gitMarker = Path.Combine(current.FullName, ".git");
      if (File.Exists(gitMarker) || Directory.Exists(gitMarker)) { return current.FullName; }

      current = current.Parent;
    }

    return null;
  }
}
