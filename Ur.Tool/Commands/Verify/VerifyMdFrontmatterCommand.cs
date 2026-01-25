using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyMdFrontmatterCommand
{
  public static Command Create(Option<string> urRootOpt)
  {
    var cmd = new Command("md-frontmatter", "Verify all *.ur.md files have required YAML frontmatter keys.");

    var allOpt = new Option<bool>("--all")
    {
      Description = "Scan all *.ur.md files in repo (default).",
      DefaultValueFactory = _ => true
    };

    cmd.Options.Add(allOpt);
    cmd.Options.Add(urRootOpt);

    cmd.SetAction(parseResult =>
    {
      var all = parseResult.GetValue(allOpt);
      _ = all;
      var urRoot = parseResult.GetValue(urRootOpt);
      var repoRoot = UrRootResolver.Resolve(urRoot);
      var result = VerifyMdFrontmatterLogic.Run(repoRoot);

      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: md-frontmatter");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: md-frontmatter");
      Environment.ExitCode = 0;
    });

    return cmd;
  }
}
