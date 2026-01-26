// ApproveModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.Commands.Approve;

public sealed record ApproveOptions(
  string PrRef,
  string ApprovedBy,
  bool NoFastForward,
  bool DeleteBranch,
  bool PruneWorktree);
