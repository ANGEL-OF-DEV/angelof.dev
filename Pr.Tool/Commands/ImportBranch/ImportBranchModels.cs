// ImportBranchModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.Commands.ImportBranch;

public sealed record ImportBranchOptions(
  string HeadBranch,
  string BaseBranch,
  string Status,
  bool GuessTurns,
  IReadOnlyList<string> TurnRefs);
