// RealmInference.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class RealmInference
{
  public static List<string> InferFromDiff(string repoRoot, IGitRunner git, string baseBranch, string headBranch, List<string> errors)
  {
    var paths = GitHelpers.DiffNameOnly(repoRoot, git, baseBranch, headBranch, errors);
    if (errors.Count > 0)
      return new List<string>();

    return InferFromPaths(paths);
  }

  public static List<string> InferFromPaths(IEnumerable<string> paths)
  {
    var realms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var path in paths)
    {
      if (path.StartsWith("[monocoque.ur]/", StringComparison.Ordinal))
        realms.Add("realm://monocoque.ur");
      else if (path.StartsWith("[monocoque.tools]/", StringComparison.Ordinal))
        realms.Add("realm://monocoque.tools");
      else if (path.StartsWith("[monocoque.workflows]/", StringComparison.Ordinal))
        realms.Add("realm://monocoque.workflows");
      else if (path.StartsWith("[monocoque.gates]/", StringComparison.Ordinal))
        realms.Add("realm://monocoque.gates");
      else if (path.StartsWith("[monocoque.prompts]/", StringComparison.Ordinal))
        realms.Add("realm://monocoque.prompts");
      else
        realms.Add("realm://root");
    }

    if (realms.Count == 0)
      realms.Add("realm://root");

    return realms.OrderBy(r => r, StringComparer.OrdinalIgnoreCase).ToList();
  }
}
