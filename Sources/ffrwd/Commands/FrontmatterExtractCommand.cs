// FrontmatterExtractCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class FrontmatterExtractCommand
  : Command<FrontmatterExtractSettings>
{
  public override int Execute(
    CommandContext             context,
    FrontmatterExtractSettings settings,
    CancellationToken          cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return FrontmatterExitCodes.RepoNotFound;
    }

    var sourcePath = ResolveSourcePath(repoRoot, settings.Path);
    var status = FrontmatterExtractor.TryExtract(sourcePath,
                                                 settings.Force,
                                                 settings.Pretty,
                                                 out var outputPath,
                                                 out var exitCode,
                                                 out var errorMessage);

    if (status == FrontmatterExtractStatus.Written)
    {
      Console.Out.WriteLine(NormalizeOutputPath(repoRoot, outputPath));
      return FrontmatterExitCodes.Success;
    }

    if (status == FrontmatterExtractStatus.Skipped) { return FrontmatterExitCodes.Success; }

    if (!string.IsNullOrWhiteSpace(errorMessage)) { Console.Error.WriteLine(errorMessage); }

    return exitCode;
  }

  private static string ResolveSourcePath(string repoRoot, string source)
  {
    var path = Path.IsPathRooted(source)
                 ? source
                 : Path.Combine(repoRoot, source);
    return Path.GetFullPath(path);
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
