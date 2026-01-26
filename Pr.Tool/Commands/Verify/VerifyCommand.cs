// VerifyCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Verify;

public static class VerifyCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("verify", "Verify a PR doc.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var prOpt = new Option<string>("--pr") { Arity = ArgumentArity.ExactlyOne };
    var allowMultiOpt = new Option<bool>("--allow-multi-realm")
    {
      Description = "Allow atomic PRs to include multiple realms."
    };

    cmd.Options.Add(prOpt);
    cmd.Options.Add(allowMultiOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new VerifyOptions(
        parseResult.GetValue(prOpt) ?? string.Empty,
        parseResult.GetValue(allowMultiOpt));
      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = VerifyLogic.Run(options, context);
      ResultPrinter.Print(result, "pr verify");
    });

    return cmd;
  }
}
