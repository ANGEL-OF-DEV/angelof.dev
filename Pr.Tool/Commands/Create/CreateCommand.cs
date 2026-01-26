// CreateCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Create;

public static class CreateCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("create", "Create a PR doc (draft or pending).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var idOpt = new Option<string>("--id") { Arity = ArgumentArity.ExactlyOne };
    var titleOpt = new Option<string>("--title") { Arity = ArgumentArity.ExactlyOne };
    var summaryOpt = new Option<string>("--summary") { Arity = ArgumentArity.ExactlyOne };
    var baseOpt = new Option<string>("--base-branch")
    {
      Description = "Base branch (default: default)."
    };
    var headOpt = new Option<string>("--head-branch")
    {
      Description = "Head branch (default: current branch)."
    };
    var statusOpt = new Option<string>("--status")
    {
      Description = "Status: draft|pending."
    };
    var realmOpt = new Option<string[]>("--realm")
    {
      Description = "Realm URI (repeatable).",
      Arity = ArgumentArity.ZeroOrMore,
      AllowMultipleArgumentsPerToken = true
    };
    var turnOpt = new Option<string[]>("--turn")
    {
      Description = "Turn ref file.repo://... (repeatable).",
      Arity = ArgumentArity.ZeroOrMore,
      AllowMultipleArgumentsPerToken = true
    };

    cmd.Options.Add(idOpt);
    cmd.Options.Add(titleOpt);
    cmd.Options.Add(summaryOpt);
    cmd.Options.Add(baseOpt);
    cmd.Options.Add(headOpt);
    cmd.Options.Add(statusOpt);
    cmd.Options.Add(realmOpt);
    cmd.Options.Add(turnOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new CreateOptions(
        parseResult.GetValue(idOpt) ?? string.Empty,
        parseResult.GetValue(titleOpt) ?? string.Empty,
        parseResult.GetValue(summaryOpt) ?? string.Empty,
        parseResult.GetValue(baseOpt) ?? "default",
        parseResult.GetValue(headOpt),
        parseResult.GetValue(statusOpt) ?? "draft",
        parseResult.GetValue(realmOpt) ?? Array.Empty<string>(),
        parseResult.GetValue(turnOpt) ?? Array.Empty<string>());

      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = CreateLogic.Run(options, context);
      ResultPrinter.Print(result, "pr create");
    });

    return cmd;
  }
}
