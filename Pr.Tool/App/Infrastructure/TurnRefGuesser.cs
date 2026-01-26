// TurnRefGuesser.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.RegularExpressions;

namespace Pr.Tool.App.Infrastructure;

public static class TurnRefGuesser
{
  private static readonly Regex TimestampRegex = new Regex("(?<ts>\\d{8}T\\d{6}Z)", RegexOptions.Compiled);

  public static List<string> GuessTurnRefs(string repoRoot, string branchName)
  {
    var match = TimestampRegex.Match(branchName);
    if (!match.Success)
      return new List<string>();

    var ts = match.Groups["ts"].Value;
    var sessionsRoot = RepoPath.ResolvePath(repoRoot, "[prompt-sessions]/sessions");
    if (!Directory.Exists(sessionsRoot))
      return new List<string>();

    var matches = Directory.GetDirectories(sessionsRoot, ts + "-*", SearchOption.TopDirectoryOnly)
      .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
      .ToList();

    if (matches.Count == 0)
      return new List<string>();

    return matches.Select(path => FileRepoUri.ToFileRepoUri(RepoPath.ToRepoRelative(repoRoot, path))).ToList();
  }
}
