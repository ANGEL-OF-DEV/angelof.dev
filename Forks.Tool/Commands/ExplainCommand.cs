// ExplainCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Forks.Tool.Services;

namespace Forks.Tool.Commands;

public static class ExplainCommand
{
  public static Command Create()
  {
    var packageArg = new Argument<string>("package")
    {
      Description = "Package to explain"
    };
    var fileOpt = new Option<string?>("--file")
    {
      Description = "Path to monocoque.forks manifest"
    };

    var cmd = new Command("explain", "Explain why a fork exists or is suggested")
    {
      TreatUnmatchedTokensAsErrors = true
    };
    cmd.Arguments.Add(packageArg);
    cmd.Options.Add(fileOpt);

    cmd.SetAction(parseResult =>
    {
      var package = parseResult.GetValue(packageArg);
      var file = parseResult.GetValue(fileOpt);
      var manifest = ForksLoader.Load(file);
      var match = manifest.Forks.FirstOrDefault(f => string.Equals(f.Package, package, StringComparison.OrdinalIgnoreCase));
      if (match == null)
      {
        Console.WriteLine($"No fork entry found for {package}.");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine($"Package: {match.Package}");
      Console.WriteLine($"Repo:    {match.Repo}");
      Console.WriteLine($"Source:  {match.Source}");
      if (!string.IsNullOrWhiteSpace(match.Motivation))
        Console.WriteLine($"Why:     {match.Motivation}");
      if (!string.IsNullOrWhiteSpace(match.Notes))
        Console.WriteLine($"Notes:   {match.Notes}");
    });

    return cmd;
  }
}
