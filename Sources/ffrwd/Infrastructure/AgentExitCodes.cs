// AgentExitCodes.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class AgentExitCodes
{
  public const int Success             = 0;
  public const int InvalidArguments    = 1;
  public const int RepoNotFound        = 2;
  public const int GitFailure          = 3;
  public const int NoAvailableIdentity = 4;
  public const int NoTasksAvailable    = 5;
}
