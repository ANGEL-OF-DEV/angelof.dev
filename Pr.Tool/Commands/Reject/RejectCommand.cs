// RejectCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Reject;

public static class RejectCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("reject", "Reject a PR and remove it from pending.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var prArg = new Argument<string>("pr")
    {
      Description = "pr://<id> or file.repo://... or repo-relative path."
    };
    var noteOpt = new Option<string>("--note") { Arity = ArgumentArity.ExactlyOne };

    cmd.Arguments.Add(prArg);
    cmd.Options.Add(noteOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new RejectOptions(
        parseResult.GetValue(prArg) ?? string.Empty,
        parseResult.GetValue(noteOpt) ?? string.Empty);

      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = RejectLogic.Run(options, context);
      ResultPrinter.Print(result, "pr reject");
    });

    return cmd;
  }
}
