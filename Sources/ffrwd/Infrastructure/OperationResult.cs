// OperationResult.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal sealed class OperationResult
{
  private OperationResult(bool success, int exitCode, string message)
  {
    Success      = success;
    ExitCode     = exitCode;
    ErrorMessage = message;
  }

  public bool   Success      { get; }
  public int    ExitCode     { get; }
  public string ErrorMessage { get; }

  public static OperationResult Ok()
  {
    return new OperationResult(true,
                               AgentExitCodes.Success,
                               string.Empty);
  }

  public static OperationResult Fail(
    string  message,
    int     exitCode,
    string? details = null)
  {
    var full = string.IsNullOrWhiteSpace(details)
                 ? message
                 : $"{message} {details}";

    return new OperationResult(false, exitCode, full);
  }
}
