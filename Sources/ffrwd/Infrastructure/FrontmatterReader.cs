// FrontmatterReader.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Text;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class FrontmatterReader
{
  public static bool TryRead(
    string      path,
    out string  yaml,
    out string? errorMessage)
  {
    yaml         = string.Empty;
    errorMessage = null;

    string content;
    try { content = File.ReadAllText(path); }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to read file. {ex.Message}";
      return false;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to read file. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to read file. {ex.Message}";
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to read file. {ex.Message}";
      return false;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to read file. {ex.Message}";
      return false;
    }

    return TryReadFrontmatter(content, out yaml, out errorMessage);
  }

  private static bool TryReadFrontmatter(
    string      content,
    out string  yaml,
    out string? errorMessage)
  {
    yaml         = string.Empty;
    errorMessage = null;

    using var reader    = new StringReader(content);
    var       firstLine = reader.ReadLine();
    if (firstLine is null || NormalizeLine(firstLine) != "---")
    {
      errorMessage = "Error: frontmatter start marker not found.";
      return false;
    }

    var     builder = new StringBuilder();
    string? line;
    while ((line = reader.ReadLine()) is not null)
    {
      if (NormalizeLine(line) == "---")
      {
        yaml = builder.ToString();
        return true;
      }

      builder.AppendLine(line);
    }

    errorMessage = "Error: frontmatter end marker not found.";
    return false;
  }

  private static string NormalizeLine(string line)
  {
    if (line.Length > 0 && line[0] == '\uFEFF') { return line[1..].Trim(); }

    return line.Trim();
  }
}
