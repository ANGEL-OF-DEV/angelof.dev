// AuthorResolver.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class AuthorResolver
{
  public static string ResolveAuthor()
  {
    var user = Environment.GetEnvironmentVariable("USER");
    if (!string.IsNullOrWhiteSpace(user))
      return user;

    var username = Environment.GetEnvironmentVariable("USERNAME");
    if (!string.IsNullOrWhiteSpace(username))
      return username;

    return Environment.UserName;
  }
}
