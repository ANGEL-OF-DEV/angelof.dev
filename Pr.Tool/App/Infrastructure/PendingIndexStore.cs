// PendingIndexStore.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class PendingIndexStore
{
  public static PendingIndex LoadOrCreate(string repoRoot, Func<DateTimeOffset> utcNow, List<string> errors)
  {
    var indexPath = RepoPath.ResolvePath(repoRoot, PrPaths.PendingIndexRel);
    if (!File.Exists(indexPath))
    {
      return new PendingIndex
      {
        SchemaVersion = "pending_prs_index.v0",
        UpdatedAtUtc = utcNow().ToString("O"),
        Pending = new List<PendingEntry>()
      };
    }

    var text = File.ReadAllText(indexPath);
    var doc = YamlHelpers.Deserialize<PendingIndex>(text, PrPaths.PendingIndexRel, errors);
    return doc ?? new PendingIndex
    {
      SchemaVersion = "pending_prs_index.v0",
      UpdatedAtUtc = utcNow().ToString("O"),
      Pending = new List<PendingEntry>()
    };
  }

  public static void Save(string repoRoot, PendingIndex index)
  {
    var yaml = YamlHelpers.Serialize(index);
    var path = RepoPath.ResolvePath(repoRoot, PrPaths.PendingIndexRel);
    EnsureDirectory(Path.GetDirectoryName(path));
    File.WriteAllText(path, yaml);
  }

  public static void Upsert(PendingIndex index, PendingEntry entry)
  {
    index.Pending.RemoveAll(p => string.Equals(p.PrUri, entry.PrUri, StringComparison.OrdinalIgnoreCase));
    index.Pending.Add(entry);
    index.Pending = index.Pending
      .OrderBy(p => p.CreatedAtUtc, StringComparer.Ordinal)
      .ThenBy(p => p.PrUri, StringComparer.OrdinalIgnoreCase)
      .ToList();
  }

  public static void Remove(PendingIndex index, string prUri)
  {
    index.Pending.RemoveAll(p => string.Equals(p.PrUri, prUri, StringComparison.OrdinalIgnoreCase));
  }

  private static void EnsureDirectory(string? path)
  {
    if (string.IsNullOrWhiteSpace(path))
      return;

    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
  }
}
