# Use MSBuild

Use Microsoft Build Engine to drive core build infrastructure in this repository.

## Use [Artifacts Output Layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output)

Build artifacts are created in `$/Artifacts` directory. <br/>
Contents are local only and excluded from soruce control.

## Define Default Project Properties (not applied automatically)

Default MSBuild Properties are defined in top-level `$/Directory.Build.props`. <br/>

```msbuild
<!-- $/Directory.Build.props -->
<Project>
  <PropertyGroup Label="Default Properties (not applied automatically)">
    <RepoRootNamespace>angelof.dev</RepoRootNamespace>
    <DefaultTargetFramework>net10.0</DefaultTargetFramework>
    <DefaultRootNamespace>$(RepoRootNamespace).$(MSBuildProjectName)</DefaultRootNamespace>
    <DefaultAssemblyName>$(DefaultRootNamespace)</DefaultAssemblyName>
  </PropertyGroup>
</Project>
```

The defaults are not automatically applied to projects in the solution. <br/>
Instead, projects reference the properties explicitly as needed. <br/>
This is to avoid changes (potentially breaking) propagating silently to projects <br/>
without a clear indication as to where they are being applied from.

```msbuild
<!-- ProjectOne.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$(DefaultTargetFramework)</TargetFramework>
    <RootNamespace>$(DefaultRootNamespace)</RootNamespace>
    <AssemblyName>$(DefaultAssemblyName)</AssemblyName>
  </PropertyGroup>
</Project>
```

## Use [Central NuGet Package Management (CPM)](https://learn.microsoft.com/nuget/consume-packages/central-package-management)

Central Package Management is enabled in `$/Directory.Packages.props`.

```msbuild
<!-- $/Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
```


## References:

- [MSBuild Documantation](https://learn.microsoft.com/visualstudio/msbuild/)
- [Use `.props` and `.targets` to cusotmize the build](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory)
