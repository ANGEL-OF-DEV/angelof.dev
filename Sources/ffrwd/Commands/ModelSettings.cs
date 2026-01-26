// ModelSettings.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using Spectre.Console;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

internal abstract class ModelSettings : CommandSettings
{
  [CommandArgument(0, "<model>")]
  public string Model { get; set; } = string.Empty;

  public override ValidationResult Validate()
  {
    return IdentityValidator.ValidateModel(Model);
  }
}
