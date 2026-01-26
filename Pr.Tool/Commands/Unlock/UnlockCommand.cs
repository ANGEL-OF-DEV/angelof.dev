// UnlockCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Unlock;

public static class UnlockCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("unlock", "Force-remove the PR lock file.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var forceOpt = new Option<bool>("--force")
    {
      Description = "Required to remove lock."
    };

    cmd.Options.Add(forceOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new UnlockOptions(parseResult.GetValue(forceOpt));
      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = UnlockLogic.Run(options, context);
      ResultPrinter.Print(result, "pr unlock");
    });

    return cmd;
  }
}
