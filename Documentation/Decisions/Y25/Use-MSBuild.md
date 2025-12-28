# Use MSBuild

Use Microsoft Build Engine to drive core build infrastructure in this repository.

## Use [Artifacts Output Layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output)

Build artifacts are created in `$/Artifacts` directory. <br/>
Contents are local only and excluded from soruce control.

## Define Standard Project Properties (applied automatically)

Standard MSBuild Properties are defined in top-level `$/Directory.Build.props`. <br/>

```msbuild
<!-- $/Directory.Build.props -->
<Project>
  <PropertyGroup Label="Standard Properties (applied automatically)">
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

The standards are automatically applied to projects in the solution. <br/>
Individual projects do not need to reference the standard properties explicitly. <br/>

> Consider defining properties as **standard** if:
> - They are unlikely to change
> - They are unlikely to change between projects in solution or directory in which they apply

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

> Consider defining properties as **default** if:
> - They are likely to change infrequently (**Target Framework Version**)
> - They are likely to be customised by some indivdiual projects (**Root Namespace**)
> - They are likely to be remain unchanged in most projects (**Assembly Name**)

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
