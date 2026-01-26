// IdentityFormat.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class IdentityFormat
{
  public static string Build(string model, int index)
  {
    var trimmed = model.Trim();
    return $"aid-{trimmed}-{index:00}";
  }

  public static string WorktreeSuffix(string identity)
  {
    return $".{identity}";
  }

  public static bool IsBranchForIdentity(string? branch, string identity)
  {
    if (string.IsNullOrWhiteSpace(branch)) { return false; }

    return branch.StartsWith($"contributors/{identity}/",
                             StringComparison.Ordinal);
  }
}
