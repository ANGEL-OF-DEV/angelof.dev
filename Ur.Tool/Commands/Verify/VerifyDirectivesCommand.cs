using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyDirectivesCommand
{
  public static Command Create()
  {
    var cmd = new Command("directives", "Verify directive ID semantics (founding IDs and consecutive positive IDs).");

    cmd.SetHandler(() =>
    {
      var repoRoot = RepoFiles.GetRepoRootOrCurrent();
      var result = VerifyDirectivesLogic.Run(repoRoot);

      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: directives");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: directives");
    });

    return cmd;
  }
}
