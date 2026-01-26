// CreateModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.Commands.Create;

public sealed record CreateOptions(
  string Id,
  string Title,
  string Summary,
  string BaseBranch,
  string? HeadBranch,
  string Status,
  IReadOnlyList<string> Realms,
  IReadOnlyList<string> TurnRefs);
