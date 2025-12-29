# Use Strict Code Policies

## Treat Warnings As Errors

**Errors** are show-stopping problems that must be fixed or otherwise remedied in order for an action or a process to progress further. <br/>

**Warnings** are treated as Errors in context of Continuous Integration and Release - they prevent integration, abort releases and make tests fail.
**Warnings** may be treated as Warnings in context of Development and Debugging, minimizing distracting interuptions to focused activity.

This behaviour is configured on MSBuild level:

```msbuild
<!-- $/Directory.Build.props -->
<Project>
  <PropertyGroup Label="Treat Warnings As Errors">
    <!--
      In DEBUG Warnings Are Warnings in order to be less distracting
      and not interrupt development flow (unless already set).
    -->
    <WarningsAreErrors Condition=" '$(WarningsAreErrors)' == '' AND '$(Configuration)' == 'Debug' ">false</WarningsAreErrors>
    <!-- In CI Warnings are always Errors. -->
    <WarningsAreErrors Condition=" '$(CI)' != '' ">true</WarningsAreErrors>
    <TreatWarningsAsErrors>$(WarningsAreErrors)</TreatWarningsAsErrors>
    <MSBuildTreatWarningsAsErrors>$(WarningsAreErrors)</MSBuildTreatWarningsAsErrors>
    <CodeAnalysisTreatWarningsAsErrors>$(WarningsAreErrors)</CodeAnalysisTreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

## Use Spellcheck

Spellchecking is cotnrolled by JetBrains Rider with a custom dictionary for this sulution is located in: `$/.idea/.idea.Solution/.idea/dictionaries/project.xml`.
