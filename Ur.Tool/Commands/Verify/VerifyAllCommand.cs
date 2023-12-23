using System.CommandLine;
using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyAllCommand
{
  public static Command Create()
  {
    var cmd = new Command("all", "Run all verifications (draft-0).");
    cmd.SetHandler(() =>
    {
      var repoRoot = RepoFiles.GetRepoRootOrCurrent();
      var failures = new List<string>();

      var md = VerifyMdFrontmatterLogic.Run(repoRoot);
      if (!md.Ok) failures.AddRange(md.Errors);

      var reg = VerifyRegistryLogic.Run(repoRoot);
      if (!reg.Ok) failures.AddRange(reg.Errors);

      var dir = VerifyDirectivesLogic.Run(repoRoot);
      if (!dir.Ok) failures.AddRange(dir.Errors);

      if (failures.Count > 0)
      {
        Console.Error.WriteLine("FAILED: verify all");
        foreach (var e in failures)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: verify all");
    });

    return cmd;
  }
}
