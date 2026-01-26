// GitRunner.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Diagnostics;

namespace Pr.Tool.App.Infrastructure;

public sealed record GitRunResult(int ExitCode, string StdOut, string StdErr)
{
  public bool Ok => ExitCode == 0;
}

public interface IGitRunner
{
  GitRunResult Run(string workingDirectory, IReadOnlyList<string> args);
}

public sealed class ProcessGitRunner : IGitRunner
{
  public GitRunResult Run(string workingDirectory, IReadOnlyList<string> args)
  {
    var startInfo = new ProcessStartInfo("git")
    {
      WorkingDirectory = workingDirectory,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    };

    foreach (var arg in args)
      startInfo.ArgumentList.Add(arg);

    using var process = Process.Start(startInfo);
    if (process is null)
      return new GitRunResult(1, string.Empty, "failed to start git");

    var stdout = process.StandardOutput.ReadToEnd();
    var stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return new GitRunResult(process.ExitCode, stdout.TrimEnd(), stderr.TrimEnd());
  }
}

public static class GitRunnerExtensions
{
  public static GitRunResult Run(this IGitRunner runner, string workingDirectory, params string[] args)
  {
    return runner.Run(workingDirectory, args);
  }
}
