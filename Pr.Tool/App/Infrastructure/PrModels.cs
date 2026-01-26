// PrModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Pr.Tool.App.Infrastructure;

public sealed class PrDoc
{
  [YamlMember(Alias = "schema_version", Order = 1)]
  public string SchemaVersion { get; set; } = "pull_request.v0";

  [YamlMember(Alias = "content_version", Order = 2)]
  public string ContentVersion { get; set; } = "0.1.0";

  [YamlMember(Alias = "id", Order = 3)]
  public string Id { get; set; } = string.Empty;

  [YamlMember(Alias = "canonical_uri", Order = 4)]
  public string CanonicalUri { get; set; } = string.Empty;

  [YamlMember(Alias = "kind", Order = 5)]
  public string Kind { get; set; } = "atomic";

  [YamlMember(Alias = "status", Order = 6)]
  public string Status { get; set; } = "draft";

  [YamlMember(Alias = "title", Order = 7)]
  public string Title { get; set; } = string.Empty;

  [YamlMember(Alias = "summary", Order = 8)]
  public string Summary { get; set; } = string.Empty;

  [YamlMember(Alias = "created_at_utc", Order = 9)]
  public string CreatedAtUtc { get; set; } = string.Empty;

  [YamlMember(Alias = "author", Order = 10)]
  public string Author { get; set; } = string.Empty;

  [YamlMember(Alias = "base", Order = 11, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public PrBranch? Base { get; set; }

  [YamlMember(Alias = "head", Order = 12, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public PrBranch? Head { get; set; }

  [YamlMember(Alias = "children", Order = 13, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public List<string>? Children { get; set; }

  [YamlMember(Alias = "merge_order", Order = 14, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public List<string>? MergeOrder { get; set; }

  [YamlMember(Alias = "realms_touched", Order = 15)]
  public List<string> RealmsTouched { get; set; } = new();

  [YamlMember(Alias = "turn_refs", Order = 16)]
  public List<string> TurnRefs { get; set; } = new();

  [YamlMember(Alias = "checks", Order = 17)]
  public List<PrCheck> Checks { get; set; } = new();

  [YamlMember(Alias = "review", Order = 18)]
  public PrReview Review { get; set; } = new();
}

public sealed class PrBranch
{
  [YamlMember(Alias = "branch", Order = 1)]
  public string Branch { get; set; } = string.Empty;
}

public sealed class PrCheck
{
  [YamlMember(Alias = "name", Order = 1)]
  public string Name { get; set; } = string.Empty;

  [YamlMember(Alias = "result", Order = 2)]
  public string Result { get; set; } = string.Empty;

  [YamlMember(Alias = "command", Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public string? Command { get; set; }
}

public sealed class PrReview
{
  [YamlMember(Alias = "approvals", Order = 1)]
  public List<PrApproval> Approvals { get; set; } = new();

  [YamlMember(Alias = "notes", Order = 2)]
  public List<string> Notes { get; set; } = new();
}

public sealed class PrApproval
{
  [YamlMember(Alias = "by", Order = 1)]
  public string By { get; set; } = string.Empty;

  [YamlMember(Alias = "at_utc", Order = 2)]
  public string AtUtc { get; set; } = string.Empty;

  [YamlMember(Alias = "note", Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
  public string? Note { get; set; }
}

public sealed class PendingIndex
{
  [YamlMember(Alias = "schema_version", Order = 1)]
  public string SchemaVersion { get; set; } = "pending_prs_index.v0";

  [YamlMember(Alias = "updated_at_utc", Order = 2)]
  public string UpdatedAtUtc { get; set; } = string.Empty;

  [YamlMember(Alias = "pending", Order = 3)]
  public List<PendingEntry> Pending { get; set; } = new();
}

public sealed class PendingEntry
{
  [YamlMember(Alias = "pr_uri", Order = 1)]
  public string PrUri { get; set; } = string.Empty;

  [YamlMember(Alias = "pr_file_repo_uri", Order = 2, ScalarStyle = ScalarStyle.DoubleQuoted)]
  public string PrFileRepoUri { get; set; } = string.Empty;

  [YamlMember(Alias = "title", Order = 3)]
  public string Title { get; set; } = string.Empty;

  [YamlMember(Alias = "kind", Order = 4)]
  public string Kind { get; set; } = string.Empty;

  [YamlMember(Alias = "created_at_utc", Order = 5)]
  public string CreatedAtUtc { get; set; } = string.Empty;

  [YamlMember(Alias = "realms_touched", Order = 6)]
  public List<string> RealmsTouched { get; set; } = new();
}
