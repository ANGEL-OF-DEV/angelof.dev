// ListCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Forks.Tool.Services;

namespace Forks.Tool.Commands;

public static class ListCommand
{
  public static Command Create()
  {
    var fileOpt = new Option<string?>("--file")
    {
      Description = "Path to monocoque.forks manifest"
    };

    var cmd = new Command("list", "Display monocoque.forks entries")
    {
      TreatUnmatchedTokensAsErrors = true
    };
    cmd.Options.Add(fileOpt);

    cmd.SetAction(parseResult =>
    {
      var manifest = ForksLoader.Load(parseResult.GetValue(fileOpt));
      foreach (var fork in manifest.Forks)
        Console.WriteLine($"{fork.Package} -> {fork.Repo} ({fork.Source})");
    });

    return cmd;
  }
}
