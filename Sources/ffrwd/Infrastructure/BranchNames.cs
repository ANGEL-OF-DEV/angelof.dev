// BranchNames.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.ffrwd.Infrastructure;

internal static class BranchNames
{
  public static string Self(string identity)
  {
    return $"contributors/{identity}/self/main";
  }

  public static string Work(string identity)
  {
    return $"contributors/{identity}/work/main";
  }
}
