using Ur.Tool.Infra;

namespace Ur.Tool.Commands.Verify;

public sealed record VerifyResult(bool Ok, IReadOnlyList<string> Errors);

public static class VerifyMdFrontmatterLogic
{
  private static readonly string[] RequiredKeys =
  [
    "id",
    "type",
    "title",
    "schema_id",
    "schema_version",
    "content_version",
    "status",
    "date",
    "steward"
  ];

  public static VerifyResult Run(string repoRoot)
  {
    var errors = new List<string>();
    var urRoot = UrRootResolver.Normalize(repoRoot);
    var files = RepoFiles.EnumerateFiles(urRoot, "*.ur.md", recursive: true);

    foreach (var file in files)
    {
      var content = File.ReadAllText(file);
      var parsed = Frontmatter.ParseYamlFrontmatter(content);

      if (!parsed.HasFrontmatter)
      {
        errors.Add($"{file}: missing YAML frontmatter (expected leading '---')");
        continue;
      }

      if (parsed.Error is not null)
        errors.Add($"{file}: {parsed.Error}");

      foreach (var key in RequiredKeys)
      {
        if (!parsed.Keys.ContainsKey(key))
          errors.Add($"{file}: missing required frontmatter key '{key}'");
      }
    }

    return new VerifyResult(errors.Count == 0, errors);
  }
}
