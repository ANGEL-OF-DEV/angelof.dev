// CreateMetaCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.CreateMeta;

public static class CreateMetaCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("create-meta", "Create a meta PR doc.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var idOpt = new Option<string>("--id") { Arity = ArgumentArity.ExactlyOne };
    var titleOpt = new Option<string>("--title") { Arity = ArgumentArity.ExactlyOne };
    var summaryOpt = new Option<string>("--summary") { Arity = ArgumentArity.ExactlyOne };
    var childrenOpt = new Option<string[]>("--children")
    {
      Arity = ArgumentArity.OneOrMore,
      AllowMultipleArgumentsPerToken = true
    };
    var statusOpt = new Option<string>("--status")
    {
      Description = "Status: draft|pending."
    };
    var mergeOrderOpt = new Option<string>("--merge-order")
    {
      Description = "Merge order override (comma-separated realms)."
    };

    cmd.Options.Add(idOpt);
    cmd.Options.Add(titleOpt);
    cmd.Options.Add(summaryOpt);
    cmd.Options.Add(childrenOpt);
    cmd.Options.Add(statusOpt);
    cmd.Options.Add(mergeOrderOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new CreateMetaOptions(
        parseResult.GetValue(idOpt) ?? string.Empty,
        parseResult.GetValue(titleOpt) ?? string.Empty,
        parseResult.GetValue(summaryOpt) ?? string.Empty,
        parseResult.GetValue(childrenOpt) ?? Array.Empty<string>(),
        parseResult.GetValue(statusOpt) ?? "draft",
        parseResult.GetValue(mergeOrderOpt));

      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = CreateMetaLogic.Run(options, context);
      ResultPrinter.Print(result, "pr create-meta");
    });

    return cmd;
  }
}
