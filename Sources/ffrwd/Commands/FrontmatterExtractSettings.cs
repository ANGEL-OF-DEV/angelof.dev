// FrontmatterExtractSettings.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class FrontmatterExtractSettings : CommandSettings
{
  [CommandArgument(0, "<path>")]
  public string Path { get; set; } = string.Empty;

  [CommandOption("--force")]
  [Description("Overwrite JSON output even if up to date.")]
  public bool Force { get; set; }

  [CommandOption("--pretty")]
  [Description("Pretty-print JSON output.")]
  public bool Pretty { get; set; }

  public override ValidationResult Validate()
  {
    if (string.IsNullOrWhiteSpace(Path)) { return ValidationResult.Error("Path is required."); }

    return ValidationResult.Success();
  }
}
