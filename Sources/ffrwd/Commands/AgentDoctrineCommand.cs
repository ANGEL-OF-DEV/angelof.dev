// AgentDoctrineCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentDoctrineCommand : Command<AgentDoctrineSettings>
{
  public override int Execute(
    CommandContext        context,
    AgentDoctrineSettings settings,
    CancellationToken     cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return AgentExitCodes.RepoNotFound;
    }

    var sourceOutput = ResolveSourceOutput(settings.Source);
    var sourcePath   = ResolveSourcePath(repoRoot, settings.Source);
    if (!DoctrineProtocolReader.TryLoad(sourcePath,
                                        out var config,
                                        out var errorMessage))
    {
      if (!string.IsNullOrWhiteSpace(errorMessage)) { Console.Error.WriteLine(errorMessage); }

      return AgentExitCodes.InvalidArguments;
    }

    var manifest = DoctrineManifestBuilder.Build(config, sourceOutput);
    var output   = DoctrineEmitFormat.FormatOutput(settings.Emit, manifest);
    Console.Out.WriteLine(output);
    return AgentExitCodes.Success;
  }

  private static string ResolveSourcePath(string repoRoot, string source)
  {
    var path = Path.IsPathRooted(source)
                 ? source
                 : Path.Combine(repoRoot, source);
    return Path.GetFullPath(path);
  }

  private static string ResolveSourceOutput(string source)
  {
    if (Path.IsPathRooted(source)) { return Path.GetFullPath(source); }

    return source;
  }
}
