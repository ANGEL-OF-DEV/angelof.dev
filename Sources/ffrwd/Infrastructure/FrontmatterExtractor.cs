// FrontmatterExtractor.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Text;

namespace angelof.dev.ffrwd.Infrastructure;

internal enum FrontmatterExtractStatus { Written, Skipped, Failed }

internal static class FrontmatterExtractor
{
  private const string FrontmatterExtension = ".yml.md";

  public static FrontmatterExtractStatus TryExtract(
    string      sourcePath,
    bool        force,
    bool        pretty,
    out string  outputPath,
    out int     exitCode,
    out string? errorMessage)
  {
    outputPath   = string.Empty;
    exitCode     = FrontmatterExitCodes.Success;
    errorMessage = null;

    if (string.IsNullOrWhiteSpace(sourcePath))
    {
      errorMessage = "Error: source path is required.";
      exitCode     = FrontmatterExitCodes.InvalidArguments;
      return FrontmatterExtractStatus.Failed;
    }

    if (!File.Exists(sourcePath))
    {
      errorMessage = $"Error: source file not found: {sourcePath}";
      exitCode     = FrontmatterExitCodes.InvalidArguments;
      return FrontmatterExtractStatus.Failed;
    }

    if (!TryGetJsonPath(sourcePath, out outputPath))
    {
      errorMessage = "Error: source must end with .yml.md.";
      exitCode     = FrontmatterExitCodes.InvalidArguments;
      return FrontmatterExtractStatus.Failed;
    }

    if (!force && ShouldSkip(sourcePath, outputPath)) { return FrontmatterExtractStatus.Skipped; }

    if (!FrontmatterReader.TryRead(sourcePath, out var yaml, out errorMessage))
    {
      exitCode = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }

    if (!FrontmatterYamlParser.TryParse(yaml, out var data, out errorMessage))
    {
      exitCode = FrontmatterExitCodes.InvalidArguments;
      return FrontmatterExtractStatus.Failed;
    }

    var json = FrontmatterJsonSerializer.Serialize(data, pretty);
    try
    {
      File.WriteAllText(outputPath,
                        json,
                        new UTF8Encoding(false));
      return FrontmatterExtractStatus.Written;
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to write JSON. {ex.Message}";
      exitCode     = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to write JSON. {ex.Message}";
      exitCode     = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to write JSON. {ex.Message}";
      exitCode     = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to write JSON. {ex.Message}";
      exitCode     = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to write JSON. {ex.Message}";
      exitCode     = FrontmatterExitCodes.IoFailure;
      return FrontmatterExtractStatus.Failed;
    }
  }

  public static bool TryGetJsonPath(string sourcePath, out string outputPath)
  {
    outputPath = string.Empty;
    if (!sourcePath.EndsWith(FrontmatterExtension,
                             StringComparison.OrdinalIgnoreCase)) { return false; }

    outputPath = sourcePath + ".json";
    return true;
  }

  private static bool ShouldSkip(string sourcePath, string outputPath)
  {
    if (!File.Exists(outputPath)) { return false; }

    var sourceTime = File.GetLastWriteTimeUtc(sourcePath);
    var outputTime = File.GetLastWriteTimeUtc(outputPath);
    return outputTime >= sourceTime;
  }
}
