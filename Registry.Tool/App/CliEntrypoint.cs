// CliEntrypoint.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using Registry.Tool.Commands.Registry;

namespace Registry.Tool.App;

public static class CliEntrypoint
{
  public static async Task<int> InvokeAsync(string[] args)
  {
    var trimmed = args.Where(a => !string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)).ToArray();

    var root = new RootCommand("registry.tool (v0) - registry bootstrap + verify")
    {
      TreatUnmatchedTokensAsErrors = true
    };

    root.Subcommands.Add(RegistryCommandFactory.Create());

    var parseResult = root.Parse(trimmed);
    return await parseResult.InvokeAsync(new InvocationConfiguration());
  }
}
