// WorktreeInitResult.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal sealed class WorktreeInitResult
{
  private WorktreeInitResult(
    bool    success,
    string  worktreePath,
    int     exitCode,
    string? errorMessage)
  {
    Success      = success;
    WorktreePath = worktreePath;
    ExitCode     = exitCode;
    ErrorMessage = errorMessage;
  }

  public bool    Success      { get; }
  public string  WorktreePath { get; }
  public int     ExitCode     { get; }
  public string? ErrorMessage { get; }

  public static WorktreeInitResult Ok(string path)
  {
    return new WorktreeInitResult(true,
                                  path,
                                  AgentExitCodes.Success,
                                  null);
  }

  public static WorktreeInitResult Fail(
    string  message,
    int     exitCode,
    string? details = null)
  {
    var full = string.IsNullOrWhiteSpace(details)
                 ? message
                 : $"{message} {details}";

    return new WorktreeInitResult(false,
                                  string.Empty,
                                  exitCode,
                                  full);
  }
}
