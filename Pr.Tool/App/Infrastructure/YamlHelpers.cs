// YamlHelpers.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Pr.Tool.App.Infrastructure;

public static class YamlHelpers
{
  public static T? Deserialize<T>(string yaml, string label, List<string> errors)
  {
    try
    {
      var deserializer = BuildDeserializer();
      var value = deserializer.Deserialize<T>(yaml);
      if (value is null)
        errors.Add($"{label} is empty or invalid");

      return value;
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
      errors.Add($"invalid YAML in {label}: {ex.Message}");
      return default;
    }
  }

  public static string Serialize<T>(T value)
  {
    var serializer = BuildSerializer();
    return serializer.Serialize(value);
  }

  private static ISerializer BuildSerializer()
  {
    return new SerializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
      .DisableAliases()
      .Build();
  }

  private static IDeserializer BuildDeserializer()
  {
    return new DeserializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .IgnoreUnmatchedProperties()
      .Build();
  }
}
