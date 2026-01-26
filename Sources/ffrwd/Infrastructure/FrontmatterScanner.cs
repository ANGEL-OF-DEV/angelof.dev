// FrontmatterScanner.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class FrontmatterScanner
{
  private const string Extension = ".yml.md";

  public static IEnumerable<string> EnumerateYmlMdFiles(
    string           repoRoot,
    GitIgnoreMatcher ignore)
  {
    var pending = new Stack<string>();
    pending.Push(repoRoot);

    while (pending.Count > 0)
    {
      var current = pending.Pop();

      foreach (var directory in Directory.EnumerateDirectories(current))
      {
        var relative = Path.GetRelativePath(repoRoot, directory);
        if (ignore.IsIgnored(relative, true)) { continue; }

        pending.Push(directory);
      }

      foreach (var file in Directory.EnumerateFiles(current))
      {
        if (!file.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)) { continue; }

        var relative = Path.GetRelativePath(repoRoot, file);
        if (ignore.IsIgnored(relative, false)) { continue; }

        yield return file;
      }
    }
  }
}
