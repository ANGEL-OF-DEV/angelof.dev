using System.CommandLine;

namespace PromptPack.Verify.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("promptpack-verify (draft-0)");

    var promptsRootOpt = new Option<string>("--prompts-root")
    {
      Description = "Prompts root directory (defaults to sibling [monocoque.prompts])."
    };
    var packRootOpt = new Option<string>("--pack-root")
    {
      Description = "Legacy alias for --prompts-root."
    };
    var cmd = new Command("verify-pack", "Verify prompt pack structure and prompt headers.");
    cmd.Options.Add(promptsRootOpt);
    cmd.Options.Add(packRootOpt);

    cmd.SetAction(parseResult =>
    {
      var promptsRoot = parseResult.GetValue(promptsRootOpt);
      var packRoot = parseResult.GetValue(packRootOpt);
      var resolvedRoot = PromptsRootResolver.Resolve(promptsRoot, packRoot);
      var result = VerifyPackLogic.Run(resolvedRoot);

      if (!result.Ok)
      {
        Console.Error.WriteLine("FAILED: verify-pack");
        foreach (var e in result.Errors)
          Console.Error.WriteLine($"- {e}");
        Environment.ExitCode = 1;
        return;
      }

      Console.WriteLine("OK: verify-pack");
      Environment.ExitCode = 0;
    });

    root.Subcommands.Add(cmd);

    var parseResult = root.Parse(trimmed);
    return await parseResult.InvokeAsync(new InvocationConfiguration());
  }
}
