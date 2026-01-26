// FakeGitRunner.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;

namespace Pr.Tool.TestSupport;

public sealed class FakeGitRunner : IGitRunner
{
  private readonly Dictionary<string, GitRunResult> _responses = new(StringComparer.Ordinal);

  public void Add(string workingDirectory, IReadOnlyList<string> args, GitRunResult result)
  {
    _responses[Key(workingDirectory, args)] = result;
  }

  public GitRunResult Run(string workingDirectory, IReadOnlyList<string> args)
  {
    var key = Key(workingDirectory, args);
    if (_responses.TryGetValue(key, out var result))
      return result;

    return new GitRunResult(1, string.Empty, "missing git response: " + key);
  }

  private static string Key(string workingDirectory, IReadOnlyList<string> args)
  {
    return workingDirectory + "::" + string.Join(" ", args);
  }
}
