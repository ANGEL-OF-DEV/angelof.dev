// RegistryCommandFactory.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Registry.Tool.Commands.BootstrapToolsV0;
using Registry.Tool.Commands.VerifyToolsV0;

namespace Registry.Tool.Commands.Registry;

public static class RegistryCommandFactory
{
  public static Command Create()
  {
    var registry = new Command("registry", "Registry maintenance commands (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var repoRootOpt = new Option<string>("--repo-root")
    {
      Description = "Repository root (defaults to current directory)."
    };
    var logSinkOpt = new Option<string>("--log-sink")
    {
      Description = "Log sink (file|stdout)."
    };
    var logFileOpt = new Option<string>("--log-file")
    {
      Description = "Repo-relative log file path (overrides default)."
    };
    var forceOpt = new Option<bool>("--force")
    {
      Description = "Allow schema rewrites beyond template-minimal edits."
    };

    registry.Subcommands.Add(BootstrapToolsV0Command.Create(repoRootOpt, logSinkOpt, logFileOpt, forceOpt));
    registry.Subcommands.Add(VerifyToolsV0Command.Create(repoRootOpt, logSinkOpt, logFileOpt));
    registry.Subcommands.Add(HelpCommand());

    return registry;
  }

  private static Command HelpCommand()
  {
    var help = new Command("help", "Show registry tool commands.");
    help.SetAction(_ =>
    {
      Console.WriteLine("registry bootstrap-tools-v0 --repo-root .");
      Console.WriteLine("registry verify-tools-v0 --repo-root .");
      Console.WriteLine("options: --log-sink stdout | --log-file [logs.local]/registry.tool/your.jsonl | --force");
    });

    return help;
  }
}
