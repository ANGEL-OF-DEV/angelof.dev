// FrontmatterYamlParser.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Globalization;
using YamlDotNet.Serialization;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class FrontmatterYamlParser
{
  public static bool TryParse(
    string      yaml,
    out object? data,
    out string? errorMessage)
  {
    data         = null;
    errorMessage = null;

    try
    {
      var deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .Build();
      var raw = deserializer.Deserialize<object>(yaml);
      data = NormalizeValue(raw);
      return true;
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to parse frontmatter. {ex.Message}";
      return false;
    }
    catch (InvalidOperationException ex)
    {
      errorMessage = $"Error: failed to parse frontmatter. {ex.Message}";
      return false;
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
      errorMessage = $"Error: failed to parse frontmatter. {ex.Message}";
      return false;
    }
  }

  private static object? NormalizeValue(object? value)
  {
    if (value is null) { return null; }

    if (value is IDictionary<object, object> objectMap)
    {
      var result = new Dictionary<string, object?>(StringComparer.Ordinal);
      foreach (var (key, mapValue) in objectMap)
      {
        var keyString = Convert.ToString(key, CultureInfo.InvariantCulture)
                     ?? string.Empty;
        result[keyString] = NormalizeValue(mapValue);
      }

      return result;
    }

    if (value is IDictionary<string, object> stringMap)
    {
      var result = new Dictionary<string, object?>(StringComparer.Ordinal);
      foreach (var (key, mapValue) in stringMap) { result[key] = NormalizeValue(mapValue); }

      return result;
    }

    if (value is IEnumerable<object> sequence)
    {
      var list = new List<object?>();
      foreach (var item in sequence) { list.Add(NormalizeValue(item)); }

      return list;
    }

    return value;
  }
}
