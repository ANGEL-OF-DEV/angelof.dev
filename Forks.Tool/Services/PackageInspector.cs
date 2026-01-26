// PackageInspector.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Forks.Tool.Services
{
  public static class PackageInspector
  {
    public static IReadOnlyList<string> GetPackageIds(string projectPath, bool includeTransitive)
    {
      var command = includeTransitive
        ? "dotnet list package --include-transitive"
        : "dotnet list package";

      var startInfo = new ProcessStartInfo("dotnet")
      {
        Arguments = command.Replace("dotnet ", string.Empty),
        WorkingDirectory = projectPath,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet process");
      var output = process.StandardOutput.ReadToEnd();
      var error = process.StandardError.ReadToEnd();
      process.WaitForExit();

      if (process.ExitCode != 0)
      {
        throw new InvalidOperationException($"dotnet list package failed: {error}");
      }

      var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      return lines
        .Where(l => l.Contains(" > ", StringComparison.Ordinal))
        .Select(l => l.Split('>', 2, StringSplitOptions.TrimEntries)[1])
        .Select(l => l.Split(' ', 2, StringSplitOptions.TrimEntries)[0])
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }
  }
}
