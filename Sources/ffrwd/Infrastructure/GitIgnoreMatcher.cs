// GitIgnoreMatcher.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Text.RegularExpressions;

namespace angelof.dev.ffrwd.Infrastructure;

internal sealed class GitIgnoreMatcher
{
  private readonly List<GitIgnoreRule> _rules;

  private GitIgnoreMatcher(List<GitIgnoreRule> rules)
  {
    _rules = rules;
  }

  public static GitIgnoreMatcher Load(string repoRoot)
  {
    var rules      = new List<GitIgnoreRule>();
    var ignorePath = Path.Combine(repoRoot, ".gitignore");
    if (!File.Exists(ignorePath)) { return new GitIgnoreMatcher(rules); }

    foreach (var rawLine in File.ReadAllLines(ignorePath))
    {
      var line = rawLine.Trim();
      if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) { continue; }

      var negated = line.StartsWith('!');
      if (negated) { line = line[1..]; }

      if (string.IsNullOrWhiteSpace(line)) { continue; }

      var directoryOnly = line.EndsWith('/');
      if (directoryOnly) { line = line.TrimEnd('/'); }

      var hasSlash = line.Contains('/', StringComparison.Ordinal);
      rules.Add(new GitIgnoreRule(line, negated, directoryOnly, hasSlash));
    }

    return new GitIgnoreMatcher(rules);
  }

  public bool IsIgnored(string relativePath, bool isDirectory)
  {
    if (_rules.Count == 0) { return false; }

    var normalized = Normalize(relativePath);
    if (string.IsNullOrWhiteSpace(normalized)) { return false; }

    var name    = Path.GetFileName(normalized);
    var ignored = false;

    foreach (var rule in _rules)
    {
      if (rule.DirectoryOnly && !isDirectory) { continue; }

      var target = rule.HasSlash ? normalized : name;
      if (GlobMatch(rule.Pattern, target)) { ignored = !rule.IsNegated; }
    }

    return ignored;
  }

  private static string Normalize(string relativePath)
  {
    var normalized = relativePath.Replace(Path.DirectorySeparatorChar,
                                          '/')
                                 .Replace(Path.AltDirectorySeparatorChar, '/');

    if (normalized.StartsWith("./", StringComparison.Ordinal)) { normalized = normalized[2..]; }

    return normalized;
  }

  private static bool GlobMatch(string pattern, string value)
  {
    var regex = "^"
              + Regex.Escape(pattern)
                     .Replace("\\*", ".*", StringComparison.Ordinal)
                     .Replace("\\?", ".",  StringComparison.Ordinal)
              + "$";
    return Regex.IsMatch(value, regex, RegexOptions.CultureInvariant);
  }

  private sealed class GitIgnoreRule
  {
    public GitIgnoreRule(
      string pattern,
      bool   isNegated,
      bool   directoryOnly,
      bool   hasSlash)
    {
      Pattern       = pattern;
      IsNegated     = isNegated;
      DirectoryOnly = directoryOnly;
      HasSlash      = hasSlash;
    }

    public string Pattern       { get; }
    public bool   IsNegated     { get; }
    public bool   DirectoryOnly { get; }
    public bool   HasSlash      { get; }
  }
}
