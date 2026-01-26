// VerifyToolsV0Command.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;

namespace Registry.Tool.Commands.VerifyToolsV0;

public static class VerifyToolsV0Command
{
  public static Command Create(Option<string> repoRootOpt)
  {
    var cmd = new Command("verify-tools-v0", "Verify tool registry + docs (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    cmd.Options.Add(repoRootOpt);

    cmd.SetAction(parseResult =>
    {
      var repoRoot = parseResult.GetValue(repoRootOpt);
      var result = VerifyToolsV0Logic.Run(repoRoot);
      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: registry verify-tools-v0");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: registry verify-tools-v0");
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
