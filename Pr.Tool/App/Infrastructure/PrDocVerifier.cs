// PrDocVerifier.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class PrDocVerifier
{
  public static void Verify(
    PrDoc doc,
    string repoRoot,
    string repoRelPath,
    IGitRunner git,
    bool allowMultiRealm,
    List<string> errors)
  {
    if (string.IsNullOrWhiteSpace(doc.SchemaVersion))
      errors.Add("schema_version is required");

    if (string.IsNullOrWhiteSpace(doc.ContentVersion))
      errors.Add("content_version is required");

    if (string.IsNullOrWhiteSpace(doc.Id))
      errors.Add("id is required");

    var pathId = PrPaths.ExtractIdFromPath(repoRelPath);
    if (!string.IsNullOrWhiteSpace(pathId) && !string.Equals(pathId, doc.Id, StringComparison.Ordinal))
      errors.Add($"id does not match filename: {doc.Id} vs {pathId}");

    var expectedUri = "pr://" + doc.Id;
    if (!string.Equals(doc.CanonicalUri, expectedUri, StringComparison.Ordinal))
      errors.Add($"canonical_uri mismatch: {doc.CanonicalUri} (expected {expectedUri})");

    if (string.Equals(doc.Kind, "atomic", StringComparison.OrdinalIgnoreCase))
    {
      if (doc.Base is null || doc.Head is null)
        errors.Add("atomic PR requires base/head branches");
    }
    else if (string.Equals(doc.Kind, "meta", StringComparison.OrdinalIgnoreCase))
    {
      if (doc.Children is null || doc.Children.Count == 0)
        errors.Add("meta PR requires children");
    }
    else
    {
      errors.Add($"invalid kind: {doc.Kind}");
    }

    if (doc.RealmsTouched.Count == 0)
      errors.Add("realms_touched is required");

    if (!allowMultiRealm && string.Equals(doc.Kind, "atomic", StringComparison.OrdinalIgnoreCase))
    {
      if (doc.RealmsTouched.Count != 1)
        errors.Add("atomic PR must touch exactly one realm (use --allow-multi-realm to override)");
    }

    if (doc.Kind.Equals("atomic", StringComparison.OrdinalIgnoreCase))
    {
      if (doc.Base is not null)
        GitHelpers.BranchExists(repoRoot, git, doc.Base.Branch, errors);

      if (doc.Head is not null)
        GitHelpers.BranchExists(repoRoot, git, doc.Head.Branch, errors);
    }

    if (doc.TurnRefs.Count > 0)
    {
      foreach (var turnRef in doc.TurnRefs)
      {
        var repoRel = FileRepoUri.ToRepoRelative(turnRef, errors);
        if (repoRel is null)
          continue;

        var full = RepoPath.ResolvePath(repoRoot, repoRel);
        if (!File.Exists(full) && !Directory.Exists(full))
          errors.Add($"turn ref not found: {turnRef}");
      }
    }

    if (doc.Kind.Equals("meta", StringComparison.OrdinalIgnoreCase) && doc.Children is not null)
    {
      var childRealms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var child in doc.Children)
      {
        if (!child.StartsWith("pr://", StringComparison.OrdinalIgnoreCase))
        {
          errors.Add($"child must be pr:// URI: {child}");
          continue;
        }

        var childId = child.Substring("pr://".Length);
        var childPath = PrDocStore.FindById(repoRoot, childId, errors);
        if (childPath is null)
          continue;

        var childDoc = PrDocStore.Load(repoRoot, childPath, errors);
        if (childDoc is null)
          continue;

        if (!string.Equals(childDoc.Status, "draft", StringComparison.OrdinalIgnoreCase)
          && !string.Equals(childDoc.Status, "pending", StringComparison.OrdinalIgnoreCase))
        {
          errors.Add($"child PR must be draft or pending: {childId}");
          continue;
        }

        foreach (var realm in childDoc.RealmsTouched)
          childRealms.Add(realm);
      }

      if (childRealms.Count > 0)
      {
        var order = doc.MergeOrder ?? MergeOrderHelper.DefaultOrder();
        foreach (var realm in childRealms)
        {
          if (!order.Contains(realm, StringComparer.OrdinalIgnoreCase))
            errors.Add($"merge_order missing realm: {realm}");
        }
      }
    }
  }
}
