using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyMdFrontmatterCommand
{
  public static Command Create()
  {
    var cmd = new Command("md-frontmatter", "Verify all *.ur.md files have required YAML frontmatter keys.");

    var allOpt = new Option<bool>("--all", () => true, "Scan all *.ur.md files in repo (default).");

    cmd.AddOption(allOpt);

    cmd.SetHandler((bool all) =>
    {
      var repoRoot = RepoFiles.GetRepoRootOrCurrent();
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
    }, allOpt);

    return cmd;
  }
}
