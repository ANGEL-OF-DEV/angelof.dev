// UrRootResolver.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Ur.Tool.Infra;

public static class UrRootResolver
{
  public static string Resolve(string? urRootOpt)
  {
    if (!string.IsNullOrWhiteSpace(urRootOpt))
      return Normalize(urRootOpt);

    var envRoot = Environment.GetEnvironmentVariable("MONOCOQUE_UR_ROOT");
    if (!string.IsNullOrWhiteSpace(envRoot))
      return Normalize(envRoot);

    var cwd = Directory.GetCurrentDirectory();
    var sibling = Path.GetFullPath(Path.Combine(cwd, "..", "[monocoque.ur]"));
    if (Directory.Exists(Path.Combine(sibling, "registry")))
      return sibling;

    var legacy = Path.Combine(cwd, "[ur]");
    if (LooksLikeUrRoot(legacy))
      return legacy;

    return cwd;
  }

  public static string Normalize(string root)
  {
    var full = Path.GetFullPath(root);
    var legacy = Path.Combine(full, "[ur]");
    if (LooksLikeUrRoot(legacy))
      return legacy;

    return full;
  }

  private static bool LooksLikeUrRoot(string path)
  {
    return Directory.Exists(Path.Combine(path, "registry"))
      || Directory.Exists(Path.Combine(path, "directives"))
      || Directory.Exists(Path.Combine(path, "bootstrap"));
  }
}
