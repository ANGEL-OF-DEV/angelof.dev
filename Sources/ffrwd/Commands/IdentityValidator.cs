// IdentityValidator.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using Spectre.Console;

namespace angelof.dev.ffrwd.Commands;

internal static class IdentityValidator
{
  public static ValidationResult ValidateModel(string model)
  {
    if (string.IsNullOrWhiteSpace(model)) { return ValidationResult.Error("Model is required."); }

    foreach (var ch in model)
    {
      if (IsAllowed(ch)) { continue; }

      return ValidationResult.Error("Model contains invalid characters.");
    }

    return ValidationResult.Success();
  }

  private static bool IsAllowed(char ch)
  {
    return char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.';
  }
}
