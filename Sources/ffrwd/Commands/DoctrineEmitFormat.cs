// DoctrineEmitFormat.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Text.Json;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace angelof.dev.ffrwd.Commands;

internal static class DoctrineEmitFormat
{
  private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

  private static readonly string[] Allowed = { "JSON", "YAML" };

  public static ValidationResult Validate(string format)
  {
    if (string.IsNullOrWhiteSpace(format))
    {
      return ValidationResult.Error("Emit format is required.");
    }

    var normalized = Normalize(format);
    if (Array.IndexOf(Allowed, normalized) >= 0) { return ValidationResult.Success(); }

    return ValidationResult.Error("Emit format must be one of: json, yaml.");
  }

  public static string FormatOutput(
    string                       format,
    IDictionary<string, object?> payload)
  {
    var resolved = Normalize(format);
    return resolved switch { "YAML" => SerializeYaml(payload), _ => SerializeJson(payload) };
  }

  private static string SerializeJson(IDictionary<string, object?> payload)
  {
    return JsonSerializer.Serialize(payload, JsonOptions);
  }

  private static string SerializeYaml(IDictionary<string, object?> payload)
  {
    var serializer = new SerializerBuilder().Build();
    return serializer.Serialize(payload);
  }

  private static string Normalize(string format)
  {
    return format.Trim().ToUpperInvariant();
  }
}
