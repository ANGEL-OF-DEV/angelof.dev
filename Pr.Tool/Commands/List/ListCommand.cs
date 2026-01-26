// ListCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.List;

public static class ListCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("list", "List draft and pending PRs.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = ListLogic.Run(new ListOptions(), context);
      foreach (var line in result.Lines)
        Console.WriteLine(line);

      ResultPrinter.Print(result.Result, "pr list");
    });

    return cmd;
  }
}
