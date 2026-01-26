// LockManager.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text;

namespace Pr.Tool.App.Infrastructure;

public sealed class LockHandle : IDisposable
{
  public string Path { get; }

  public LockHandle(string path)
  {
    Path = path;
  }

  public void Dispose()
  {
    if (File.Exists(Path))
      File.Delete(Path);
  }
}

public static class LockManager
{
  public static LockHandle? Acquire(string repoRoot, string command, List<string> errors)
  {
    var lockPath = ResolveLockPath(repoRoot);
    var lockDir = System.IO.Path.GetDirectoryName(lockPath);
    if (!string.IsNullOrWhiteSpace(lockDir) && !Directory.Exists(lockDir))
      Directory.CreateDirectory(lockDir);

    try
    {
      using var stream = new FileStream(lockPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
      var content = BuildContent(repoRoot, command);
      var bytes = Encoding.UTF8.GetBytes(content);
      stream.Write(bytes, 0, bytes.Length);
      stream.Flush(true);
      return new LockHandle(lockPath);
    }
    catch (IOException)
    {
      errors.Add($"lock busy: {lockPath}");
      return null;
    }
    catch (UnauthorizedAccessException ex)
    {
      errors.Add($"lock access denied: {ex.Message}");
      return null;
    }
  }

  public static string ResolveLockPath(string repoRoot)
  {
    return System.IO.Path.Combine(repoRoot, ".git", "monocoque", "locks", "pr.tool.lock");
  }

  public static string BuildContent(string repoRoot, string command)
  {
    var created = DateTimeOffset.UtcNow.ToString("O");
    var pid = Environment.ProcessId;
    var user = Environment.UserName;
    var host = Environment.MachineName;

    return string.Join("\n", new[]
    {
      $"created_at_utc: {created}",
      $"command: {command}",
      $"pid: {pid}",
      $"user: {user}",
      $"host: {host}",
      $"repo_root: {repoRoot}",
      string.Empty
    });
  }
}
