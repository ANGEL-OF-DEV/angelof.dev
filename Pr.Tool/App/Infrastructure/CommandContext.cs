// CommandContext.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Logging;

namespace Pr.Tool.App.Infrastructure;

public sealed record CommandContext(
  string WorkingDirectory,
  IGitRunner Git,
  LogOptions LogOptions,
  Func<DateTimeOffset> UtcNow);

public static class CommandContextFactory
{
  public static CommandContext CreateDefault(LogOptions logOptions)
  {
    return new CommandContext(
      Directory.GetCurrentDirectory(),
      new ProcessGitRunner(),
      logOptions,
      () => DateTimeOffset.UtcNow);
  }
}
