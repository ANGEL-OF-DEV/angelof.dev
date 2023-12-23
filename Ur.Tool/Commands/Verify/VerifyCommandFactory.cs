using System.CommandLine;

namespace Ur.Tool.Commands.Verify;

public static class VerifyCommandFactory
{
  public static Command Create()
  {
    var verify = new Command("verify", "Verification commands");

    verify.AddCommand(VerifyMdFrontmatterCommand.Create());
    verify.AddCommand(VerifyRegistryCommand.Create());
    verify.AddCommand(VerifyDirectivesCommand.Create());
    verify.AddCommand(VerifyAllCommand.Create());

    return verify;
  }
}
