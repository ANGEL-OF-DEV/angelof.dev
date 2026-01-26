using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyRegistryCommand
{
  public static Command Create(Option<string> urRootOpt)
  {
    var cmd = new Command("registry", "Verify registry uses canonical ur/... paths and contains no [ur]/ physical paths.");

    cmd.Options.Add(urRootOpt);

    cmd.SetAction(parseResult =>
    {
      var urRoot = parseResult.GetValue(urRootOpt);
      var repoRoot = UrRootResolver.Resolve(urRoot);
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
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
