// MergeOrderHelper.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class MergeOrderHelper
{
  public static List<string> DefaultOrder()
  {
    return new List<string>
    {
      "realm://monocoque.ur",
      "realm://monocoque.tools",
      "realm://monocoque.workflows",
      "realm://monocoque.gates",
      "realm://monocoque.prompts",
      "realm://root"
    };
  }

  public static List<string>? Parse(string? input, List<string> errors)
  {
    if (string.IsNullOrWhiteSpace(input))
      return null;

    var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (parts.Length == 0)
      return null;

    var realms = new List<string>();
    foreach (var part in parts)
    {
      if (part.StartsWith("realm://", StringComparison.OrdinalIgnoreCase))
      {
        realms.Add(part);
        continue;
      }

      var mapped = part switch
      {
        "ur" => "realm://monocoque.ur",
        "tools" => "realm://monocoque.tools",
        "workflows" => "realm://monocoque.workflows",
        "gates" => "realm://monocoque.gates",
        "prompts" => "realm://monocoque.prompts",
        "root" => "realm://root",
        _ => string.Empty
      };

      if (string.IsNullOrWhiteSpace(mapped))
      {
        errors.Add($"unknown merge-order realm token: {part}");
        return null;
      }

      realms.Add(mapped);
    }

    return realms;
  }
}
