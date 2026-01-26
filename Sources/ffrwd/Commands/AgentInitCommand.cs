// AgentInitCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using LibGit2Sharp;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentInitCommand : Command<AgentInitSettings>
{
  public override int Execute(
    CommandContext    context,
    AgentInitSettings settings,
    CancellationToken cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return AgentExitCodes.RepoNotFound;
    }

    try
    {
      GlobalSettings.SetExtensions("relativeworktrees");
      using var repo = new Repository(repoRoot);
      var result = WorktreeInitializer.Initialize(repo,
                                                  settings.Model);

      if (!result.Success)
      {
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
          Console.Error.WriteLine(result.ErrorMessage);
        }

        return result.ExitCode;
      }

      var output = EmitFormat.FormatOutput(settings.Emit,
                                           result.WorktreePath);
      Console.Out.WriteLine(output);
      return AgentExitCodes.Success;
    }
    catch (LibGit2SharpException ex)
    {
      Console.Error.WriteLine($"Error: failed to open repository. {ex.Message}");
      return AgentExitCodes.RepoNotFound;
    }
  }
}
