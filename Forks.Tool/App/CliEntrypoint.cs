// CliEntrypoint.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Forks.Tool.Commands;

namespace Forks.Tool.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("forks (draft-0) - monocoque.forks helper")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    root.Subcommands.Add(ListCommand.Create());
    root.Subcommands.Add(SuggestCommand.Create());
    root.Subcommands.Add(DiffCommand.Create());
    root.Subcommands.Add(SyncCommand.Create());
    root.Subcommands.Add(ExplainCommand.Create());

    var parseResult = root.Parse(trimmed);
    return await parseResult.InvokeAsync(new InvocationConfiguration());
  }
}
