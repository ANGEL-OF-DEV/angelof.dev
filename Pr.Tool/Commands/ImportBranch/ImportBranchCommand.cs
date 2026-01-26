// ImportBranchCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.ImportBranch;

public static class ImportBranchCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("import-branch", "Hydrate a PR doc from a branch diff.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var headOpt = new Option<string>("--head") { Arity = ArgumentArity.ExactlyOne };
    var baseOpt = new Option<string>("--base")
    {
      Description = "Base branch (default: default)."
    };
    var statusOpt = new Option<string>("--status")
    {
      Description = "Status: draft|pending."
    };
    var guessTurnsOpt = new Option<bool?>("--guess-turns")
    {
      Description = "Try to infer turn refs from branch name."
    };
    var turnOpt = new Option<string[]>("--turn")
    {
      Description = "Turn ref file.repo://... (repeatable).",
      Arity = ArgumentArity.ZeroOrMore,
      AllowMultipleArgumentsPerToken = true
    };

    cmd.Options.Add(headOpt);
    cmd.Options.Add(baseOpt);
    cmd.Options.Add(statusOpt);
    cmd.Options.Add(guessTurnsOpt);
    cmd.Options.Add(turnOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var guessTurns = parseResult.GetValue(guessTurnsOpt) ?? true;
      var options = new ImportBranchOptions(
        parseResult.GetValue(headOpt) ?? string.Empty,
        parseResult.GetValue(baseOpt) ?? "default",
        parseResult.GetValue(statusOpt) ?? "draft",
        guessTurns,
        parseResult.GetValue(turnOpt) ?? Array.Empty<string>());

      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = ImportBranchLogic.Run(options, context);
      ResultPrinter.Print(result, "pr import-branch");
    });

    return cmd;
  }
}
