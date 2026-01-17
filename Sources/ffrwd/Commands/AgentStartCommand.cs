// AgentStartCommand.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.ffrwd.Infrastructure;
using LibGit2Sharp;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd.Commands;

[SuppressMessage("Performance",
                 "CA1812:Avoid uninstantiated internal classes",
                 Justification = "Instantiated by Spectre.Console.Cli.")]
internal sealed class AgentStartCommand : Command<AgentStartSettings>
{
  public override int Execute(
    CommandContext     context,
    AgentStartSettings settings,
    CancellationToken  cancellationToken)
  {
    var repoRoot = RepositoryLocator.FindRoot();
    if (repoRoot is null)
    {
      Console.Error.WriteLine("Error: repo root not found.");
      return AgentExitCodes.RepoNotFound;
    }

    if (!TryExtractFrontmatter(repoRoot,
                               settings.Force,
                               settings.Pretty,
                               out var rootExtractExitCode,
                               out var rootExtractError))
    {
      if (!string.IsNullOrWhiteSpace(rootExtractError))
      {
        Console.Error.WriteLine(rootExtractError);
      }

      return rootExtractExitCode;
    }

    string worktreePath;
    try
    {
      GlobalSettings.SetExtensions("relativeworktrees");
      using var repo = new Repository(repoRoot);
      var result = WorktreeInitializer.Initialize(repo,
                                                  settings.Model);

      if (!result.Success)
      {
        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
          Console.Error.WriteLine(result.ErrorMessage);
        }

        return result.ExitCode;
      }

      worktreePath = result.WorktreePath;
    }
    catch (LibGit2SharpException ex)
    {
      Console.Error.WriteLine($"Error: failed to open repository. {ex.Message}");
      return AgentExitCodes.RepoNotFound;
    }

    if (!TryExtractFrontmatter(worktreePath,
                               settings.Force,
                               settings.Pretty,
                               out var frontmatterExitCode,
                               out var frontmatterError))
    {
      if (!string.IsNullOrWhiteSpace(frontmatterError))
      {
        Console.Error.WriteLine(frontmatterError);
      }

      return frontmatterExitCode;
    }

    if (!TryLoadDoctrine(worktreePath,
                         repoRoot,
                         settings.Source,
                         out var doctrineExitCode,
                         out var doctrineError))
    {
      if (!string.IsNullOrWhiteSpace(doctrineError)) { Console.Error.WriteLine(doctrineError); }

      return doctrineExitCode;
    }

    var output = EmitFormat.FormatOutput(settings.Emit,
                                         worktreePath);
    Console.Out.WriteLine(output);
    return AgentExitCodes.Success;
  }

  private static bool TryExtractFrontmatter(
    string      worktreePath,
    bool        force,
    bool        pretty,
    out int     exitCode,
    out string? errorMessage)
  {
    exitCode     = FrontmatterExitCodes.Success;
    errorMessage = null;

    var ignore = GitIgnoreMatcher.Load(worktreePath);
    foreach (var file in FrontmatterScanner.EnumerateYmlMdFiles(worktreePath,
                                                                ignore))
    {
      var status = FrontmatterExtractor.TryExtract(file,
                                                   force,
                                                   pretty,
                                                   out _,
                                                   out var fileExitCode,
                                                   out var fileError);

      if (status == FrontmatterExtractStatus.Failed)
      {
        exitCode     = fileExitCode;
        errorMessage = fileError;
        return false;
      }
    }

    return true;
  }

  private static bool TryLoadDoctrine(
    string      worktreePath,
    string      repoRoot,
    string      source,
    out int     exitCode,
    out string? errorMessage)
  {
    exitCode     = AgentExitCodes.Success;
    errorMessage = null;

    if (TryLoadDoctrineAt(worktreePath,
                          source,
                          out var config,
                          out var error,
                          out var primaryExists,
                          out var primaryPath))
    {
      _ = DoctrineManifestBuilder.Build(config,
                                        ResolveSourceOutput(source));
      return true;
    }

    if (primaryExists)
    {
      errorMessage = error;
      exitCode     = AgentExitCodes.InvalidArguments;
      return false;
    }

    if (Path.IsPathRooted(source)
     || PathsMatch(worktreePath, repoRoot))
    {
      errorMessage = error;
      exitCode     = AgentExitCodes.InvalidArguments;
      return false;
    }

    if (TryLoadDoctrineAt(repoRoot,
                          source,
                          out var fallbackConfig,
                          out var fallbackError,
                          out var fallbackExists,
                          out var fallbackPath))
    {
      _ = DoctrineManifestBuilder.Build(fallbackConfig,
                                        ResolveSourceOutput(source));
      return true;
    }

    if (!fallbackExists)
    {
      errorMessage =
        $"Error: doctrine protocol not found: {primaryPath}. "
      + $"Fallback not found: {fallbackPath}.";
    }
    else { errorMessage = fallbackError; }

    exitCode = AgentExitCodes.InvalidArguments;
    return false;
  }

  private static bool TryLoadDoctrineAt(
    string                     root,
    string                     source,
    out DoctrineProtocolConfig config,
    out string?                errorMessage,
    out bool                   fileExists,
    out string                 resolvedPath)
  {
    resolvedPath = ResolveSourcePath(root, source);
    fileExists   = File.Exists(resolvedPath);
    if (!fileExists)
    {
      config       = new DoctrineProtocolConfig();
      errorMessage = $"Error: doctrine protocol not found: {resolvedPath}";
      return false;
    }

    if (!DoctrineProtocolReader.TryLoad(resolvedPath,
                                        out config,
                                        out errorMessage)) { return false; }

    return true;
  }

  private static bool PathsMatch(string left, string right)
  {
    var leftFull = Path.GetFullPath(left)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var rightFull = Path.GetFullPath(right)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var comparison = OperatingSystem.IsWindows()
                       ? StringComparison.OrdinalIgnoreCase
                       : StringComparison.Ordinal;

    return string.Equals(leftFull, rightFull, comparison);
  }

  private static string ResolveSourcePath(string repoRoot, string source)
  {
    var path = Path.IsPathRooted(source)
                 ? source
                 : Path.Combine(repoRoot, source);
    return Path.GetFullPath(path);
  }

  private static string ResolveSourceOutput(string source)
  {
    if (Path.IsPathRooted(source)) { return Path.GetFullPath(source); }

    return source;
  }
}
