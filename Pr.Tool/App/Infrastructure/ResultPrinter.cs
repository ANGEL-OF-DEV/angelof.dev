// ResultPrinter.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace Pr.Tool.App.Infrastructure;

public static class ResultPrinter
{
  public static void Print(CommandResult result, string commandName)
  {
    if (!result.Ok)
    {
      Console.Error.WriteLine($"FAILED: {commandName}");
      foreach (var error in result.Errors)
        Console.Error.WriteLine($"- {error}");
      Environment.ExitCode = 1;
      return;
    }

    if (result.Warnings.Count > 0)
    {
      foreach (var warning in result.Warnings)
        Console.Error.WriteLine($"WARN: {warning}");
    }

    Console.WriteLine($"OK: {commandName}");
    Environment.ExitCode = 0;
  }
}
