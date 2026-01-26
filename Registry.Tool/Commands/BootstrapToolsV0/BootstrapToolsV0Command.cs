// BootstrapToolsV0Command.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Registry.Tool.App.Logging;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public static class BootstrapToolsV0Command
{
  public static Command Create(Option<string> repoRootOpt, Option<string> logSinkOpt, Option<string> logFileOpt, Option<bool> forceOpt)
  {
    var cmd = new Command("bootstrap-tools-v0", "Create/update tool registry + schemas (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    cmd.Options.Add(repoRootOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);
    cmd.Options.Add(forceOpt);

    cmd.SetAction(parseResult =>
    {
      var repoRoot = parseResult.GetValue(repoRootOpt);
      var logSink = parseResult.GetValue(logSinkOpt);
      var logFile = parseResult.GetValue(logFileOpt);
      var force = parseResult.GetValue(forceOpt);
      var result = BootstrapToolsV0Logic.Run(new BootstrapOptions(repoRoot, force, new LogOptions(logSink, logFile)));
      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: registry bootstrap-tools-v0");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      var useStdout = string.Equals(logSink, "stdout", StringComparison.OrdinalIgnoreCase);
      if (useStdout)
        Console.Error.WriteLine("OK: registry bootstrap-tools-v0");
      else
        Console.WriteLine("OK: registry bootstrap-tools-v0");
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
