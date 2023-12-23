using System.CommandLine;

namespace PromptPack.Verify.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("promptpack-verify (draft-0)");

    var packRootOpt = new Option<string>("--pack-root", () => ".", "Pack root directory.");
    var cmd = new Command("verify-pack", "Verify prompt pack structure and prompt headers.");
    cmd.AddOption(packRootOpt);

    cmd.SetHandler((string packRoot) =>
    {
      var result = VerifyPackLogic.Run(packRoot);

      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: verify-pack");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: verify-pack");
    }, packRootOpt);

    root.AddCommand(cmd);

    return await root.InvokeAsync(trimmed);
  }
}
