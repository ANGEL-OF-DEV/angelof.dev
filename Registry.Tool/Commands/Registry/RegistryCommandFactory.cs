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

    registry.Subcommands.Add(BootstrapToolsV0Command.Create(repoRootOpt));
    registry.Subcommands.Add(VerifyToolsV0Command.Create(repoRootOpt));
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
    });

    return help;
  }
}
