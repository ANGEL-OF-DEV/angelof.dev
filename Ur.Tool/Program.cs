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
    // Explicit opt-in app mode
    if (args.Any(a => string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)))
    {
      return await RunAppAsync(args);
    }

    // Default: run tests (safe for IDE + dotnet test)
    return await RunTestsAsync(args);
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
