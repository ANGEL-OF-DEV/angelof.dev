using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using TUnit.Engine.Framework;
using Ur.Tool.App;

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
    // 1. META FLAGS (terminal, highest priority)
    var metaFlags = new[] { "--help", "-h", "--version", "-v", "--schema", "--explain" };
    if (args.Any(a => metaFlags.Contains(a, StringComparer.OrdinalIgnoreCase)))
      return "help";

    // 2. TEST FLAGS (explicit opt-out)
    var testFlags = new[] { "--test", "--validate", "--lint", "--check" };
    if (args.Any(a => testFlags.Contains(a, StringComparer.OrdinalIgnoreCase)))
      return "test";

    // 3. --plan (requires operational args)
    if (args.Any(a => string.Equals(a, "--plan", StringComparison.OrdinalIgnoreCase)))
    {
      if (HasOperationalArgs(args))
        return "app";
      Console.Error.WriteLine("error: --plan requires operational arguments");
      return "help";
    }

    // 4. --app (explicit, legacy)
    if (args.Any(a => string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)))
      return "app";

    // 5. Operational arguments (implicit app)
    if (HasOperationalArgs(args))
      return "app";

    // 6. Default to help
    return "help";
  }

  private static bool HasOperationalArgs(string[] args)
  {
    var metaFlags = new[] { "--help", "-h", "--version", "-v", "--schema", "--explain", "--test", "--validate", "--lint", "--check", "--plan", "--app", "--verbose" };
    return args.Any(a => !a.StartsWith("-") || (a.StartsWith("-") && !metaFlags.Contains(a, StringComparer.OrdinalIgnoreCase)));
  }

  private static int RunHelp()
  {
    Console.WriteLine("Ur.Tool — Universal command/tool runner");
    Console.WriteLine();
    Console.WriteLine("Usage: ur <command> [args]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  ur run <tool>          Run a tool from registry");
    Console.WriteLine("  ur verify <spec>       Verify tool/command output");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --help                 Show this help message");
    Console.WriteLine("  --version              Show version");
    Console.WriteLine("  --test                 Run tests (not app mode)");
    return 0;
  }

  private static async Task<int> RunAppAsync(string[] args)
  {
    return await CliEntrypoint.InvokeAsync(args);
  }

  private static async Task<int> RunTestsAsync(string[] args)
  {
    var builder = await TestApplication.CreateBuilderAsync(args);

    // This wires up extensions registered via MSBuild/NuGet (including TUnit).
    TestingPlatformBuilderHook.AddExtensions(builder, args);

    using var app = await builder.BuildAsync();
    return await app.RunAsync();
  }
}
