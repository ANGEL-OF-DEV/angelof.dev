// AgentStartSettings.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentStartSettings : ModelSettings
{
  [CommandOption("--emit <format>")]
  [Description("Output format: auto|path|sh|pwsh|cmd.")]
  public string Emit { get; set; } = "auto";

  [CommandOption("--force")]
  [Description("Overwrite JSON output even if up to date.")]
  public bool Force { get; set; }

  [CommandOption("--pretty")]
  [Description("Pretty-print JSON output.")]
  public bool Pretty { get; set; }

  [CommandOption("--source <path>")]
  [Description("Doctrine protocol JSON path, relative to repo root.")]
  public string Source { get; set; }
    = "Doctrine/Prindiples-And-Protocols.yml.md.json";

  public override ValidationResult Validate()
  {
    var baseResult = base.Validate();
    if (!baseResult.Successful) { return baseResult; }

    if (string.IsNullOrWhiteSpace(Source))
    {
      return ValidationResult.Error("Source path is required.");
    }

    if (!Source.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    {
      return ValidationResult.Error("Source must be a .json file.");
    }

    return EmitFormat.Validate(Emit);
  }
}
