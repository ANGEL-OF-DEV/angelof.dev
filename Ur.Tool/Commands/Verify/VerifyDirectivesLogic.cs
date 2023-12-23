using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public static class VerifyDirectivesLogic
{
  public static VerifyResult Run(string repoRoot)
  {
    var errors = new List<string>();

    var directivesDir = Path.Combine(repoRoot, "[ur]", "directives");
    if (!Directory.Exists(directivesDir))
      return new VerifyResult(false, new[] { $"Missing directives directory: {directivesDir}" });

    var files = Directory.EnumerateFiles(directivesDir, "*.ur.md", SearchOption.TopDirectoryOnly).ToArray();
    if (files.Length == 0)
      return new VerifyResult(false, new[] { $"No directives found in: {directivesDir}" });

    var ids = new Dictionary<int, string>();
    foreach (var file in files)
    {
      var content = File.ReadAllText(file);
      var parsed = Frontmatter.ParseYamlFrontmatter(content);
      if (!parsed.HasFrontmatter)
      {
        errors.Add($"{file}: missing YAML frontmatter");
        continue;
      }

      if (!parsed.Keys.TryGetValue("id", out var idStr) || !int.TryParse(idStr, out var id))
      {
        errors.Add($"{file}: missing/invalid integer frontmatter key 'id'");
        continue;
      }

      if (ids.ContainsKey(id))
        errors.Add($"Duplicate directive id {id}: {file} and {ids[id]}");
      else
        ids[id] = file;
    }

    var required = new[] { -3, -2, -1, 0 };
    foreach (var r in required)
      if (!ids.ContainsKey(r))
        errors.Add($"Missing required founding directive id {r} in [ur]/directives");

    var positives = ids.Keys.Where(k => k > 0).OrderBy(k => k).ToArray();
    if (positives.Length > 0)
    {
      if (positives[0] != 1)
        errors.Add($"Directive IDs must start at 1 after founding; first positive id was {positives[0]}");

      for (var i = 0; i < positives.Length; i++)
      {
        var expected = i + 1;
        if (positives[i] != expected)
        {
          errors.Add($"Directive IDs must be consecutive with no gaps; expected {expected} but found {positives[i]}");
          break;
        }
      }
    }

    return new VerifyResult(errors.Count == 0, errors);
  }
}
