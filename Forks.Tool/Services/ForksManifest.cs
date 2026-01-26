// ForksManifest.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Forks.Tool.Services
{
  public record ForksManifest
  (
    [property: JsonPropertyName("forks")] IReadOnlyList<ForkRecord> Forks
  );
}
