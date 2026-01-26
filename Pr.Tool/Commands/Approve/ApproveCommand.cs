// ApproveCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;

namespace Pr.Tool.Commands.Approve;

public static class ApproveCommand
{
  public static Command Create(Option<string> logSinkOpt, Option<string> logFileOpt)
  {
    var cmd = new Command("approve", "Approve and merge a pending PR.")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var prArg = new Argument<string>("pr")
    {
      Description = "pr://<id> or file.repo://... or repo-relative path."
    };
    var approvedByOpt = new Option<string>("--approved-by") { Arity = ArgumentArity.ExactlyOne };
    var noffOpt = new Option<bool>("--no-ff")
    {
      Description = "Use --no-ff merge."
    };
    var deleteBranchOpt = new Option<bool>("--delete-branch")
    {
      Description = "Delete head branch after merge."
    };
    var pruneWorktreeOpt = new Option<bool>("--prune-worktree")
    {
      Description = "Remove job worktree under [monocoque.dev-branches] if found."
    };

    cmd.Arguments.Add(prArg);
    cmd.Options.Add(approvedByOpt);
    cmd.Options.Add(noffOpt);
    cmd.Options.Add(deleteBranchOpt);
    cmd.Options.Add(pruneWorktreeOpt);
    cmd.Options.Add(logSinkOpt);
    cmd.Options.Add(logFileOpt);

    cmd.SetAction(parseResult =>
    {
      var options = new ApproveOptions(
        parseResult.GetValue(prArg) ?? string.Empty,
        parseResult.GetValue(approvedByOpt) ?? string.Empty,
        parseResult.GetValue(noffOpt),
        parseResult.GetValue(deleteBranchOpt),
        parseResult.GetValue(pruneWorktreeOpt));

      var logOptions = new LogOptions(parseResult.GetValue(logSinkOpt), parseResult.GetValue(logFileOpt));
      var context = CommandContextFactory.CreateDefault(logOptions);
      var result = ApproveLogic.Run(options, context);
      ResultPrinter.Print(result, "pr approve");
    });

    return cmd;
  }
}
