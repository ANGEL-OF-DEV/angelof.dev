using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyRegistryLogic
{
  public static VerifyResult Run(string repoRoot)
  {
    var urRoot = UrRootResolver.Normalize(repoRoot);
    var errors = new List<string>();

    var registryPath = Path.Combine(urRoot, "registry", "v0", "registry.yaml");
    if (!File.Exists(registryPath))
      return new VerifyResult(false, new[] { $"Missing registry file: {registryPath}" });

    var text = File.ReadAllText(registryPath);

    if (text.Contains("[ur]/", StringComparison.Ordinal) || text.Contains("[ur]\\", StringComparison.Ordinal))
      errors.Add($"{registryPath}: registry MUST NOT contain physical path prefix '[ur]/' (canonical paths only)");

    // crude checks: any 'path:' line must start with 'ur/'
    var lines = text.Split('\n');
    for (var i = 0; i < lines.Length; i++)
    {
      var line = lines[i].TrimStart();
      if (!line.StartsWith("path:", StringComparison.Ordinal)) continue;

      var value = line.Substring("path:".Length).Trim();
      if (!value.StartsWith("ur/", StringComparison.Ordinal))
        errors.Add($"{registryPath}:{i+1}: path must start with canonical 'ur/': {value}");
    }

    return new VerifyResult(errors.Count == 0, errors);
  }
}
