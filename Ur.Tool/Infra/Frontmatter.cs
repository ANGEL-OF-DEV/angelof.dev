using System.Text;

namespace Ur.Tool.Infra;

public sealed record FrontmatterParseResult(
  bool HasFrontmatter,
  IReadOnlyDictionary<string, string> Keys,
  string? Error
);

public static class Frontmatter
{
  public static FrontmatterParseResult ParseYamlFrontmatter(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
      return new FrontmatterParseResult(false, new Dictionary<string, string>(), "Empty file");

    using var reader = new StringReader(content);

    var first = reader.ReadLine();
    if (!string.Equals(first?.Trim(), "---", StringComparison.Ordinal))
      return new FrontmatterParseResult(false, new Dictionary<string, string>(), null);

    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    string? line;
    while ((line = reader.ReadLine()) is not null)
    {
      var t = line.Trim();
      if (t == "---")
        return new FrontmatterParseResult(true, dict, null);

      // Very small YAML subset: key: value (top-level only)
      var idx = t.IndexOf(':');
      if (idx <= 0) continue;

      var key = t.Substring(0, idx).Trim();
      var value = t.Substring(idx + 1).Trim().Trim('"');
      if (key.Length == 0) continue;

      if (!dict.ContainsKey(key))
        dict[key] = value;
    }

    return new FrontmatterParseResult(true, dict, "Frontmatter not terminated with '---'");
  }
}
