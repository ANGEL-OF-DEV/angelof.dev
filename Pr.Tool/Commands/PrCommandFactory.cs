// PrCommandFactory.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.Commands.Approve;
using Pr.Tool.Commands.Create;
using Pr.Tool.Commands.CreateMeta;
using Pr.Tool.Commands.ImportBranch;
using Pr.Tool.Commands.List;
using Pr.Tool.Commands.Reject;
using Pr.Tool.Commands.Submit;
using Pr.Tool.Commands.Unlock;
using Pr.Tool.Commands.Verify;

namespace Pr.Tool.Commands;

public static class PrCommandFactory
{
  public static Command Create()
  {
    var pr = new Command("pr", "Local PR doc commands (v0).")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    var logSinkOpt = new Option<string>("--log-sink")
    {
      Description = "Log sink (file|stdout)."
    };
    var logFileOpt = new Option<string>("--log-file")
    {
      Description = "Repo-relative log file path (overrides default)."
    };

    pr.Subcommands.Add(CreateCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(SubmitCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(ListCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(VerifyCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(ImportBranchCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(CreateMetaCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(ApproveCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(RejectCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(UnlockCommand.Create(logSinkOpt, logFileOpt));
    pr.Subcommands.Add(HelpCommand());

    return pr;
  }

  private static Command HelpCommand()
  {
    var help = new Command("help", "Show pr tool commands.");
    help.SetAction(_ =>
    {
      Console.WriteLine("pr create --id <id> --title <title> --summary <summary> [--status draft|pending]");
      Console.WriteLine("pr submit --id <id>");
      Console.WriteLine("pr list");
      Console.WriteLine("pr verify --pr pr://<id>");
      Console.WriteLine("pr import-branch --head <branch>");
      Console.WriteLine("pr create-meta --id <id> --title <title> --summary <summary> --children pr://...");
      Console.WriteLine("pr approve pr://<id> --approved-by <id>");
      Console.WriteLine("pr reject pr://<id> --note <note>");
      Console.WriteLine("pr unlock --force");
      Console.WriteLine("options: --log-sink stdout | --log-file [logs.local]/pr.tool/your.jsonl");
    });

    return help;
  }
}
