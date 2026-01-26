// PrPaths.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class PrPaths
{
  public const string PendingIndexRel = "PENDING-PRS.v0.yaml";
  public const string PrRootRel = "[prs]";
  public const string DraftRel = "[prs]/draft";
  public const string PendingRel = "[prs]/pending";
  public const string ApprovedRel = "[prs]/approved";
  public const string RejectedRel = "[prs]/rejected";

  public static string BuildPrFileName(string id)
  {
    return id + ".pr.yaml";
  }

  public static string BuildDraftRel(string id) => $"{DraftRel}/{BuildPrFileName(id)}";
  public static string BuildPendingRel(string id) => $"{PendingRel}/{BuildPrFileName(id)}";
  public static string BuildApprovedRel(string id) => $"{ApprovedRel}/{BuildPrFileName(id)}";
  public static string BuildRejectedRel(string id) => $"{RejectedRel}/{BuildPrFileName(id)}";

  public static string? ExtractIdFromPath(string path)
  {
    var file = Path.GetFileName(path);
    if (!file.EndsWith(".pr.yaml", StringComparison.OrdinalIgnoreCase))
      return null;

    return file.Substring(0, file.Length - ".pr.yaml".Length);
  }
}
