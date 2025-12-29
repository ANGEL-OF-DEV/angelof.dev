# Use MSTest

[MSTest](https://github.com/microsoft/testfx) is the Microsoft Test Framework that works with .NET CLI, Visual Studio (+Code), and Rider. <br/>
It supports [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro).

## Use MSTest.Sdk

A MSBuild project SDK includes all the recommended packages and boilerplate configuration. <br/>
It is shipped as a NuGet package but should not be installed as a regular package dependency. <br/>
Instead, it should be set directly as Test Project Sdk (MSBuild property):

```msbuild
<Project Sdk="MSTest.Sdk"></Project>
```

## Use MSTest.TestFramework and MSTest.Analyzers for testing infrastructure projects

Testing infrastructure projects intended as shared dependencies of multiple test projects
[should install `MSTest.TestFramework` and `MSTest.Analyzers` NuGet packages directly](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-getting-started)
and [set `IsTestApplication` to `false`](https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-sdk#test-utility-helper-libraries):

```msbuild
<Project>
  <PropertyGroup>
    <IsTestApplication>false</IsTestApplication>
  </PropertyGroup>
</Project>
```

## References:

- [MSTest (github.com/microsoft/testfx)](https://github.com/microsoft/testfx)
- [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
