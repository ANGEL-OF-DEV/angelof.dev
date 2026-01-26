// AgentDoctrineSettings.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentDoctrineSettings : CommandSettings
{
  [CommandOption("--emit <format>")]
  [Description("Output format: json|yaml.")]
  public string Emit { get; set; } = "json";

  [CommandOption("--source <path>")]
  [Description("Doctrine protocol JSON path, relative to repo root.")]
  public string Source { get; set; }
    = "Doctrine/Prindiples-And-Protocols.yml.md.json";

  public override ValidationResult Validate()
  {
    if (string.IsNullOrWhiteSpace(Source))
    {
      return ValidationResult.Error("Source path is required.");
    }

    if (!Source.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    {
      return ValidationResult.Error("Source must be a .json file.");
    }

    return DoctrineEmitFormat.Validate(Emit);
  }
}
