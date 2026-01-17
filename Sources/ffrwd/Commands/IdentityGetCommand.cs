// IdentityGetCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class IdentityGetCommand : Command<IdentityGetSettings>
{
  public override int Execute(
    CommandContext      context,
    IdentityGetSettings settings,
    CancellationToken   cancellationToken)
  {
    var identity = IdentityFormat.Build(settings.Model, 0);
    Console.Out.WriteLine(identity);
    return IdentityExitCodes.Success;
  }
}
