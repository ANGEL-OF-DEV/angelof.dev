// AgentInitSettings.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentInitSettings : ModelSettings
{
  [CommandOption("--emit <format>")]
  [Description("Output format: auto|path|sh|pwsh|cmd.")]
  public string Emit { get; set; } = "auto";

  public override ValidationResult Validate()
  {
    var baseResult = base.Validate();
    if (!baseResult.Successful) { return baseResult; }

    return EmitFormat.Validate(Emit);
  }
}
