// Program.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System;
using System.Linq;
using System.Threading.Tasks;
using Forks.Tool.App;
using Microsoft.Testing.Platform.Builder;
using TUnit.Engine.Framework;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    var mode = ResolveMode(args);
    return mode switch
    {
      "app" => await RunAppAsync(args),
      "test" => await RunTestsAsync(args),
      "help" => RunHelp(),
      _ => await RunTestsAsync(args)
    };
  }

  private static string ResolveMode(string[] args)
  {
    var metaFlags = new[] { "--help", "-h", "--version", "-v", "--schema", "--explain" };
    if (args.Any(a => metaFlags.Contains(a, StringComparer.OrdinalIgnoreCase)))
      return "help";

    var testFlags = new[] { "--test", "--validate", "--lint", "--check" };
    if (args.Any(a => testFlags.Contains(a, StringComparer.OrdinalIgnoreCase)))
      return "test";

    if (args.Any(a => string.Equals(a, "--plan", StringComparison.OrdinalIgnoreCase)))
    {
      if (HasOperationalArgs(args))
        return "app";
      Console.Error.WriteLine("error: --plan requires operational arguments");
      return "help";
    }

    if (args.Any(a => string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)))
      return "app";

    if (HasOperationalArgs(args))
      return "app";

    return "help";
  }

  private static bool HasOperationalArgs(string[] args)
  {
    var metaFlags = new[] { "--help", "-h", "--version", "-v", "--schema", "--explain", "--test", "--validate", "--lint", "--check", "--plan", "--app", "--verbose" };
    return args.Any(a => !a.StartsWith("-") || (a.StartsWith("-") && !metaFlags.Contains(a, StringComparer.OrdinalIgnoreCase)));
  }

  private static int RunHelp()
  {
    Console.WriteLine("monocoque forks");
    Console.WriteLine();
    Console.WriteLine("Usage: forks <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  list                 Display monocoque.forks entries");
    Console.WriteLine("  suggest              Suggest missing fork entries from NuGet deps");
    Console.WriteLine("  diff                 Compare suggestions vs current forks");
    Console.WriteLine("  sync                 Clone listed forks locally (optional)");
    Console.WriteLine("  explain <package>    Explain why a fork is listed");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --help               Show this help message");
    Console.WriteLine("  --version            Show version");
    Console.WriteLine("  --test               Run tests (not app mode)");
    return 0;
  }

  private static async Task<int> RunAppAsync(string[] args)
  {
    return await CliEntrypoint.InvokeAsync(args);
  }

  private static async Task<int> RunTestsAsync(string[] args)
  {
    var builder = await TestApplication.CreateBuilderAsync(args);
    TestingPlatformBuilderHook.AddExtensions(builder, args);

    using var app = await builder.BuildAsync();
    return await app.RunAsync();
  }
}
