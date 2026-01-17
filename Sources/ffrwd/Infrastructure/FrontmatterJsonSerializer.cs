// FrontmatterJsonSerializer.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Text.Json;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class FrontmatterJsonSerializer
{
  private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

  private static readonly JsonSerializerOptions CompactOptions = new() { WriteIndented = false };

  public static string Serialize(object? payload, bool pretty)
  {
    var options = pretty ? PrettyOptions : CompactOptions;
    return JsonSerializer.Serialize(payload, options);
  }
}
