// SyncCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Forks.Tool.Services;

namespace Forks.Tool.Commands;

public static class SyncCommand
{
  public static Command Create()
  {
    var fileOpt = new Option<string?>("--file")
    {
      Description = "Path to monocoque.forks manifest"
    };
    var destinationOpt = new Option<string>("--destination")
    {
      Description = "Directory to clone forks into",
      Arity = ArgumentArity.ZeroOrOne
    };

    var dryRunOpt = new Option<bool>("--dry-run")
    {
      Description = "Show what would happen without cloning",
      Arity = ArgumentArity.ZeroOrOne
    };

    var cmd = new Command("sync", "Clone forks locally (optional)")
    {
      TreatUnmatchedTokensAsErrors = true
    };
    cmd.Options.Add(fileOpt);
    cmd.Options.Add(destinationOpt);
    cmd.Options.Add(dryRunOpt);

    cmd.SetAction(parseResult =>
    {
      var file = parseResult.GetValue(fileOpt);
      var destination = parseResult.GetValue(destinationOpt) ?? "monocoque.forks";
      var dryRun = parseResult.GetValue(dryRunOpt);

      var manifest = ForksLoader.Load(file);
      foreach (var fork in manifest.Forks)
      {
        var targetPath = Path.Combine(destination, fork.Package);
        if (dryRun)
        {
          Console.WriteLine($"would clone {fork.Repo} -> {targetPath}");
          continue;
        }

        Console.WriteLine($"TODO clone {fork.Repo} -> {targetPath}");
      }
    });

    return cmd;
  }
}
