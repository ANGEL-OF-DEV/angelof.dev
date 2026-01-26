// BootstrapToolsV0Command.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public static class BootstrapToolsV0Command
{
  public static Command Create(Option<string> repoRootOpt)
  {
    var cmd = new Command("bootstrap-tools-v0", "Create/update tool registry + schemas (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    cmd.Options.Add(repoRootOpt);

    cmd.SetAction(parseResult =>
    {
      var repoRoot = parseResult.GetValue(repoRootOpt);
      var result = BootstrapToolsV0Logic.Run(repoRoot);
      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: registry bootstrap-tools-v0");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: registry bootstrap-tools-v0");
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
