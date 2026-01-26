// SuggestCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Forks.Tool.Services;

namespace Forks.Tool.Commands;

public static class SuggestCommand
{
  public static Command Create()
  {
    var fileOpt = new Option<string?>("--file")
    {
      Description = "Path to monocoque.forks manifest"
    };
    var projectOpt = new Option<string>("--project")
    {
      Description = "Path to project for dotnet list package"
    };
    var includeTransitiveOpt = new Option<bool>("--include-transitive")
    {
      Description = "Include transitive packages",
      Arity = ArgumentArity.ZeroOrOne
    };

    var cmd = new Command("suggest", "Suggest missing fork entries based on NuGet packages")
    {
      TreatUnmatchedTokensAsErrors = true
    };
    cmd.Options.Add(fileOpt);
    cmd.Options.Add(projectOpt);
    cmd.Options.Add(includeTransitiveOpt);

    cmd.SetAction(parseResult =>
    {
      var file = parseResult.GetValue(fileOpt);
      var project = parseResult.GetValue(projectOpt);
      var includeTransitive = parseResult.GetValue(includeTransitiveOpt);

      if (string.IsNullOrWhiteSpace(project))
      {
        Console.Error.WriteLine("error: --project is required");
        Environment.ExitCode = 1;
        return;
      }

      var manifest = ForksLoader.Load(file);
      var packages = PackageInspector.GetPackageIds(project, includeTransitive);
      var suggestions = SuggestionEngine.Suggest(packages, manifest.Forks);

      if (suggestions.Count == 0)
      {
        Console.WriteLine("OK: no missing fork entries");
        Environment.ExitCode = 0;
        return;
      }

      foreach (var s in suggestions)
        Console.WriteLine($"{s.Package} -> [missing]");

      Environment.ExitCode = 1;
    });

    return cmd;
  }
}
