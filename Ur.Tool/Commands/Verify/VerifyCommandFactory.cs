using System.CommandLine;

namespace Ur.Tool.Commands.Verify;

public static class VerifyCommandFactory
{
  public static Command Create()
  {
    var verify = new Command("verify", "Verification commands");

    var urRootOpt = new Option<string>("--ur-root")
    {
      Description = "UR root directory (defaults to sibling [monocoque.ur])."
    };

    verify.Subcommands.Add(VerifyMdFrontmatterCommand.Create(urRootOpt));
    verify.Subcommands.Add(VerifyRegistryCommand.Create(urRootOpt));
    verify.Subcommands.Add(VerifyDirectivesCommand.Create(urRootOpt));
    verify.Subcommands.Add(VerifyAllCommand.Create(urRootOpt));

    return verify;
  }
}
