// EmitFormat.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using Spectre.Console;

namespace angelof.dev.ffrwd.Commands;

internal static class EmitFormat
{
  private static readonly string[] Allowed = { "AUTO", "PATH", "SH", "PWSH", "CMD" };

  public static ValidationResult Validate(string format)
  {
    if (string.IsNullOrWhiteSpace(format))
    {
      return ValidationResult.Error("Emit format is required.");
    }

    var normalized = Normalize(format);
    if (Array.IndexOf(Allowed, normalized) >= 0) { return ValidationResult.Success(); }

    return ValidationResult.Error("Emit format must be one of: auto, path, sh, pwsh, cmd.");
  }

  public static string FormatOutput(string format, string path)
  {
    var resolved = Resolve(format);
    return resolved switch
           {
             "PATH" => path,
             "SH"   => $"cd \"{EscapeForDoubleQuotes(path,           '\\')}\"",
             "PWSH" => $"Set-Location \"{EscapeForDoubleQuotes(path, '`')}\"",
             "CMD"  => $"cd /d \"{path}\"",
             _      => path
           };
  }

  private static string Resolve(string format)
  {
    var normalized = Normalize(format);
    if (normalized != "AUTO") { return normalized; }

    return OperatingSystem.IsWindows() ? "PWSH" : "SH";
  }

  private static string Normalize(string format)
  {
    return format.Trim().ToUpperInvariant();
  }

  private static string EscapeForDoubleQuotes(string value, char escape)
  {
    if (string.IsNullOrEmpty(value)) { return value; }

    return value.Replace("\"",
                         $"{escape}\"",
                         StringComparison.Ordinal);
  }
}
