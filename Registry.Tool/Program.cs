// Program.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Registry.Tool.App;
using TUnit.Engine.Framework;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    if (args.Any(a => string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)))
    {
      return await RunAppAsync(args);
    }

    return await RunTestsAsync(args);
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
