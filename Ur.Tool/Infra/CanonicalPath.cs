namespace Ur.Tool.Infra;

public static class CanonicalPath
{
  public const string CanonicalUrPrefix = "ur/";
  public const string PhysicalUrPrefix = "[ur]/";

  public static string CanonicalToPhysical(string canonicalPath)
  {
    if (!canonicalPath.StartsWith(CanonicalUrPrefix, StringComparison.Ordinal))
      throw new ArgumentException($"Expected canonical path starting with '{CanonicalUrPrefix}': {canonicalPath}");

    var relative = canonicalPath.Substring(CanonicalUrPrefix.Length);
    return PhysicalUrPrefix + relative.Replace('\\', '/');
  }

  public static bool LooksPhysical(string path) => path.Contains("[ur]", StringComparison.Ordinal);
}
