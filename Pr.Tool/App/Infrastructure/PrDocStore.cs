// PrDocStore.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class PrDocStore
{
  public static PrDoc? Load(string repoRoot, string repoRelPath, List<string> errors)
  {
    var path = RepoPath.ResolvePath(repoRoot, repoRelPath);
    if (!File.Exists(path))
    {
      errors.Add($"PR doc not found: {repoRelPath}");
      return null;
    }

    var text = File.ReadAllText(path);
    return YamlHelpers.Deserialize<PrDoc>(text, repoRelPath, errors);
  }

  public static void Save(string repoRoot, string repoRelPath, PrDoc doc)
  {
    var path = RepoPath.ResolvePath(repoRoot, repoRelPath);
    EnsureDirectory(Path.GetDirectoryName(path));
    var yaml = YamlHelpers.Serialize(doc);
    File.WriteAllText(path, yaml);
  }

  public static string? ResolvePathFromInput(string repoRoot, string input, List<string> errors)
  {
    if (input.StartsWith("pr://", StringComparison.OrdinalIgnoreCase))
    {
      var id = input.Substring("pr://".Length);
      return FindById(repoRoot, id, errors);
    }

    if (input.StartsWith("file.repo://", StringComparison.OrdinalIgnoreCase))
    {
      var repoRel = input.Substring("file.repo://".Length);
      return repoRel;
    }

    if (RepoPath.IsRepoRelative(input))
      return input;

    errors.Add($"pr reference must be pr:// or file.repo:// or repo-relative path: {input}");
    return null;
  }

  public static string? FindById(string repoRoot, string id, List<string> errors)
  {
    var candidates = new[]
    {
      PrPaths.BuildPendingRel(id),
      PrPaths.BuildDraftRel(id),
      PrPaths.BuildApprovedRel(id),
      PrPaths.BuildRejectedRel(id)
    };

    foreach (var candidate in candidates)
    {
      var path = RepoPath.ResolvePath(repoRoot, candidate);
      if (File.Exists(path))
        return candidate;
    }

    errors.Add($"PR doc not found for id: {id}");
    return null;
  }

  public static string ToFileRepoUri(string repoRel)
  {
    return FileRepoUri.ToFileRepoUri(repoRel);
  }

  private static void EnsureDirectory(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return;

    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
  }
}
