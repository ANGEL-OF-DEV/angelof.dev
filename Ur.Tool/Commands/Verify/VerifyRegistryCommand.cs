using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyRegistryCommand
{
  public static Command Create()
  {
    var cmd = new Command("registry", "Verify registry uses canonical ur/... paths and contains no [ur]/ physical paths.");

    cmd.SetHandler(() =>
    {
      var repoRoot = RepoFiles.GetRepoRootOrCurrent();
      var result = VerifyRegistryLogic.Run(repoRoot);

      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: registry");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: registry");
    });

    return cmd;
  }
}
