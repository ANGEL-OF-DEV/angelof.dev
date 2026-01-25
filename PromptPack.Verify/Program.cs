using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using PromptPack.Verify.App;
using TUnit.Engine.Framework;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    if (args.Any(a => string.Equals(a, "--app", StringComparison.OrdinalIgnoreCase)))
      return await CliEntrypoint.InvokeAsync(args);

    var builder = await TestApplication.CreateBuilderAsync(args);
    TestingPlatformBuilderHook.AddExtensions(builder, args);
    using var app = await builder.BuildAsync();
    return await app.RunAsync();
  }
}
