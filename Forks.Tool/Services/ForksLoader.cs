// ForksLoader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.IO;
using System.Text.Json;

namespace Forks.Tool.Services
{
  public static class ForksLoader
  {
    public static ForksManifest Load(string? path)
    {
      var manifestPath = ResolvePath(path);
      using var stream = File.OpenRead(manifestPath);
      var manifest = JsonSerializer.Deserialize<ForksManifest>(stream, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
      if (manifest == null)
      {
        throw new InvalidDataException($"Failed to parse forks manifest at '{manifestPath}'.");
      }

      return manifest;
    }

    private static string ResolvePath(string? path)
    {
      if (!string.IsNullOrEmpty(path))
      {
        if (File.Exists(path))
        {
          return path;
        }
        throw new FileNotFoundException($"Specified manifest not found: {path}");
      }

      var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "monocoque.forks.json");
      if (File.Exists(defaultPath))
      {
        return defaultPath;
      }

      throw new FileNotFoundException("No monocoque.forks manifest found. Provide --file or run from repo root.");
    }
  }
}
