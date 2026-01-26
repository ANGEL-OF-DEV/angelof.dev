// TaskIdAllocator.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace angelof.dev.ffrwd.Infrastructure;

internal static class TaskIdAllocator
{
  private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

  [SuppressMessage("Usage",
                   "CA2000:Use recommended dispose pattern",
                   Justification = "Stream disposed via explicit using block.")]
  public static bool TryReserve(
    string      path,
    out long    taskId,
    out string? errorMessage)
  {
    taskId       = 0;
    errorMessage = null;

    if (string.IsNullOrWhiteSpace(path))
    {
      errorMessage = "Error: sequence file path is required.";
      return false;
    }

    var directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrWhiteSpace(directory))
    {
      try { Directory.CreateDirectory(directory); }
      catch (ArgumentException ex)
      {
        errorMessage = $"Error: failed to create sequence directory. {ex.Message}";
        return false;
      }
      catch (IOException ex)
      {
        errorMessage = $"Error: failed to create sequence directory. {ex.Message}";
        return false;
      }
      catch (NotSupportedException ex)
      {
        errorMessage = $"Error: failed to create sequence directory. {ex.Message}";
        return false;
      }
      catch (UnauthorizedAccessException ex)
      {
        errorMessage = $"Error: failed to create sequence directory. {ex.Message}";
        return false;
      }
      catch (System.Security.SecurityException ex)
      {
        errorMessage = $"Error: failed to create sequence directory. {ex.Message}";
        return false;
      }
    }

    var        lockPath = BuildLockPath(path, directory);
    FileStream lockStream;
    try
    {
      lockStream = new FileStream(lockPath,
                                  FileMode.OpenOrCreate,
                                  FileAccess.ReadWrite,
                                  FileShare.None);
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }

    using (lockStream)
    {
      TaskIdSequenceState state;
      if (!File.Exists(path)) { state = new TaskIdSequenceState(); }
      else
      {
        string content;
        try { content = File.ReadAllText(path); }
        catch (ArgumentException ex)
        {
          errorMessage = $"Error: failed to read sequence file. {ex.Message}";
          return false;
        }
        catch (IOException ex)
        {
          errorMessage = $"Error: failed to read sequence file. {ex.Message}";
          return false;
        }
        catch (NotSupportedException ex)
        {
          errorMessage = $"Error: failed to read sequence file. {ex.Message}";
          return false;
        }
        catch (UnauthorizedAccessException ex)
        {
          errorMessage = $"Error: failed to read sequence file. {ex.Message}";
          return false;
        }
        catch (System.Security.SecurityException ex)
        {
          errorMessage = $"Error: failed to read sequence file. {ex.Message}";
          return false;
        }

        try
        {
          var parsed = JsonSerializer.Deserialize<TaskIdSequenceState>(content,
            JsonOptions);
          if (parsed is null)
          {
            errorMessage = "Error: sequence file JSON missing.";
            return false;
          }

          state = parsed;
        }
        catch (JsonException ex)
        {
          errorMessage = $"Error: failed to parse sequence JSON. {ex.Message}";
          return false;
        }
        catch (ArgumentException ex)
        {
          errorMessage = $"Error: failed to parse sequence JSON. {ex.Message}";
          return false;
        }
        catch (InvalidOperationException ex)
        {
          errorMessage = $"Error: failed to parse sequence JSON. {ex.Message}";
          return false;
        }
        catch (NotSupportedException ex)
        {
          errorMessage = $"Error: failed to parse sequence JSON. {ex.Message}";
          return false;
        }
      }

      if (state.NextId < 1)
      {
        errorMessage = "Error: sequence next_id must be >= 1.";
        return false;
      }

      if (state.LastId < 0)
      {
        errorMessage = "Error: sequence last_id must be >= 0.";
        return false;
      }

      if (state.LastId >= state.NextId)
      {
        errorMessage = "Error: sequence next_id must be > last_id.";
        return false;
      }

      taskId       = state.NextId;
      state.LastId = taskId;
      state.NextId = taskId + 1;

      var json     = JsonSerializer.Serialize(state, JsonOptions);
      var tempPath = BuildTempPath(path, directory);
      try
      {
        File.WriteAllText(tempPath, json, new UTF8Encoding(false));
        File.Move(tempPath, path, true);
        return true;
      }
      catch (ArgumentException ex)
      {
        errorMessage = $"Error: failed to write sequence JSON. {ex.Message}";
        return false;
      }
      catch (IOException ex)
      {
        errorMessage = $"Error: failed to write sequence JSON. {ex.Message}";
        return false;
      }
      catch (NotSupportedException ex)
      {
        errorMessage = $"Error: failed to write sequence JSON. {ex.Message}";
        return false;
      }
      catch (UnauthorizedAccessException ex)
      {
        errorMessage = $"Error: failed to write sequence JSON. {ex.Message}";
        return false;
      }
      catch (System.Security.SecurityException ex)
      {
        errorMessage = $"Error: failed to write sequence JSON. {ex.Message}";
        return false;
      }
      finally { TryDeleteTemp(tempPath); }
    }
  }

  private static string BuildLockPath(string path, string? directory)
  {
    var fileName = Path.GetFileName(path);
    if (string.IsNullOrWhiteSpace(fileName)) { return path + ".lock"; }

    if (string.IsNullOrWhiteSpace(directory)) { return path + ".lock"; }

    return Path.Combine(directory, $"{fileName}.lock");
  }

  private static string BuildTempPath(string path, string? directory)
  {
    var fileName = Path.GetFileName(path);
    if (string.IsNullOrWhiteSpace(fileName)) { fileName = "sequence.json"; }

    var tempName = $"{fileName}.tmp";
    if (string.IsNullOrWhiteSpace(directory)) { return tempName; }

    return Path.Combine(directory, tempName);
  }

  private static void TryDeleteTemp(string path)
  {
    try
    {
      if (File.Exists(path)) { File.Delete(path); }
    }
    catch (IOException) { }
    catch (UnauthorizedAccessException) { }
    catch (System.Security.SecurityException) { }
  }

  private static bool TryOpenLockStream(
    string          lockPath,
    out FileStream? stream,
    out string?     errorMessage)
  {
    stream       = null;
    errorMessage = null;
    try
    {
      stream = new FileStream(lockPath,
                              FileMode.OpenOrCreate,
                              FileAccess.ReadWrite,
                              FileShare.None);
      return true;
    }
    catch (ArgumentException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (IOException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (NotSupportedException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
    catch (System.Security.SecurityException ex)
    {
      errorMessage = $"Error: failed to lock sequence file. {ex.Message}";
      return false;
    }
  }
}

internal sealed class TaskIdSequenceState
{
  [JsonPropertyName("next_id")]
  public long NextId { get; set; } = 1;

  [JsonPropertyName("last_id")]
  public long LastId { get; set; }
}
