// CreateMetaModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.Commands.CreateMeta;

public sealed record CreateMetaOptions(
  string Id,
  string Title,
  string Summary,
  IReadOnlyList<string> Children,
  string Status,
  string? MergeOrder);
