// CommandResult.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public sealed record CommandResult(
  bool Ok,
  IReadOnlyList<string> Errors,
  IReadOnlyList<string> Warnings,
  IReadOnlyList<string> Decisions,
  IReadOnlyList<string> Edits)
{
  public static CommandResult Success(
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits)
    => new(true, Array.Empty<string>(), warnings, decisions, edits);

  public static CommandResult Failure(
    IReadOnlyList<string> errors,
    IReadOnlyList<string> warnings,
    IReadOnlyList<string> decisions,
    IReadOnlyList<string> edits)
    => new(false, errors, warnings, decisions, edits);
}
