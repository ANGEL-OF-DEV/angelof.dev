using System;
using System.Diagnostics.CodeAnalysis;
using TUnit.Core;

[assembly: Retry(3)]
[assembly: ExcludeFromCodeCoverage]

namespace Monocoque;

public class GlobalHooks
{
  [Before(TestSession)]
  public static void SetUp()
  {
    Console.WriteLine(@"...before everything!");
    OptOutFromTelemetry();
  }

  private static void OptOutFromTelemetry()
  {
    Environment.SetEnvironmentVariable("TESTINGPLATFORM_TELEMETRY_OPTOUT", "1");
    Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
  }

  [After(TestSession)]
  public static void CleanUp()
  {
    Console.WriteLine(@"...and after!");
  }
}
