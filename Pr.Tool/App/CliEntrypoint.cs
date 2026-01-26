// CliEntrypoint.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Pr.Tool.Commands;

namespace Pr.Tool.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("pr.tool (v0) - local PR docs + approvals")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    root.Subcommands.Add(PrCommandFactory.Create());

    var parseResult = root.Parse(trimmed);
    return await parseResult.InvokeAsync(new InvocationConfiguration());
  }
}
