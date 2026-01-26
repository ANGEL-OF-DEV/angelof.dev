// FrontmatterExtractAllCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class FrontmatterExtractAllCommand
  : Command<FrontmatterExtractAllSettings>
{
  public override int Execute(
    CommandContext                context,
    FrontmatterExtractAllSettings settings,
    CancellationToken             cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return FrontmatterExitCodes.RepoNotFound;
    }

    var ignore   = GitIgnoreMatcher.Load(repoRoot);
    var failed   = false;
    var exitCode = FrontmatterExitCodes.Success;

    foreach (var file in FrontmatterScanner.EnumerateYmlMdFiles(repoRoot,
                                                                ignore))
    {
      var status = FrontmatterExtractor.TryExtract(file,
                                                   settings.Force,
                                                   settings.Pretty,
                                                   out var outputPath,
                                                   out var fileExitCode,
                                                   out var errorMessage);

      if (status == FrontmatterExtractStatus.Written)
      {
        Console.Out.WriteLine(NormalizeOutputPath(repoRoot, outputPath));
        continue;
      }

      if (status == FrontmatterExtractStatus.Skipped) { continue; }

      failed   = true;
      exitCode = fileExitCode;
      if (!string.IsNullOrWhiteSpace(errorMessage)) { Console.Error.WriteLine(errorMessage); }
    }

    return failed ? exitCode : FrontmatterExitCodes.Success;
  }

  private static string NormalizeOutputPath(string repoRoot, string path)
  {
    var root = Path.GetFullPath(repoRoot)
                   .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var fullPath = Path.GetFullPath(path);
    var comparison = OperatingSystem.IsWindows()
                       ? StringComparison.OrdinalIgnoreCase
                       : StringComparison.Ordinal;

    if (fullPath.StartsWith(root + Path.DirectorySeparatorChar, comparison)
     || string.Equals(fullPath, root, comparison)) { return Path.GetRelativePath(root, fullPath); }

    return fullPath;
  }
}
