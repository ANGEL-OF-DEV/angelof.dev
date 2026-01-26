// SubmitCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Submit;

public static class SubmitCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("submit", "Move a draft PR into pending state.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var idOpt = new Option<string>("--id") { Arity = ArgumentArity.ExactlyOne };

    cmd.Options.Add(idOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new SubmitOptions(parseResult.GetValue(idOpt) ?? string.Empty);
      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = SubmitLogic.Run(options, context);
      ResultPrinter.Print(result, "pr submit");
    });

    return cmd;
  }
}
