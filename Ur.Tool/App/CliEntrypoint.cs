using System.CommandLine;
using Ur.Tool.Commands.Verify;

namespace Ur.Tool.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    // Strip the sentinel --app (leave everything else)
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("urtool (draft-0) - local-first governance verifier");

    root.AddCommand(VerifyCommandFactory.Create());

    return await root.InvokeAsync(trimmed);
  }
}
