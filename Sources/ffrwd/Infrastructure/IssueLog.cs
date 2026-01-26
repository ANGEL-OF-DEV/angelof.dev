// IssueLog.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class IssueLog
{
  public static void Record(string message)
  {
    // TODO(issue-log): persist once issues log exists.
    Console.Error.WriteLine(string.Concat("TODO(issue-log): ", message));
  }
}
