// FileRepoUri.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class FileRepoUri
{
  private const string Prefix = "file.repo://";

  public static bool IsFileRepoUri(string value)
  {
    return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
  }

  public static string ToFileRepoUri(string repoRelative)
  {
    return Prefix + repoRelative;
  }

  public static string? ToRepoRelative(string uri, List<string> errors)
  {
    if (!IsFileRepoUri(uri))
    {
      errors.Add($"turn ref must be file.repo://...: {uri}");
      return null;
    }

    return uri.Substring(Prefix.Length);
  }
}
