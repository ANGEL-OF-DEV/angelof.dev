// VerifyToolsV0Command.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Registry.Tool.App.Logging;

namespace Registry.Tool.Commands.VerifyToolsV0;

public static class VerifyToolsV0Command
{
  public static Command Create(Option<string> repoRootOpt, Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("verify-tools-v0", "Verify tool registry + docs (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    cmd.Options.Add(repoRootOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var repoRoot = parseResult.GetValue(repoRootOpt);
      var logSink = parseResult.GetValue(logSinkOpt);
      var logFile = parseResult.GetValue(logFileOpt);
      var result = VerifyToolsV0Logic.Run(new VerifyOptions(repoRoot, new LogOptions(logSink, logFile)));
      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: registry verify-tools-v0");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      var useStdout = string.Equals(logSink, "stdout", StringComparison.OrdinalIgnoreCase);
      if (useStdout)
        Console.Error.WriteLine("OK: registry verify-tools-v0");
      else
        Console.WriteLine("OK: registry verify-tools-v0");
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
