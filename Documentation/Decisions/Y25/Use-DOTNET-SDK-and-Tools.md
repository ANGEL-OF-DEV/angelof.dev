# Use .NET SDK and Tools

## Use [`global.json`](https://learn.microsoft.com/dotnet/core/tools/global-json)

Define SDK versions required by the solution in `$/global.json`.

> NOTE: Version numbers [MUST be **the full version numbers**](https://learn.microsoft.com/dotnet/core/tools/global-json#version). <br/>
> Example: `10.0.101`

## Use strict SDK versioning policies

.NET SDK development moves quickly and as a result things sometimes break or new/unexpected behaviour gets introduced.
Those kind of problems are time consuming to diagnose, while at the same time the latest features are not immediately
used of in existing codebase. For those and to help ensure repeatable builds, the exact versions of SDKs are pinned (up to Patch component)
and rollForward is disabled, as are the pre-release versions.

```json
// $/global.json
{
  "sdk": {
    "version": "10.0.101",
    "rollForward": "disable",
    "allowPrerelease": false
  }
}
```

This applies to **Continuous Integration and Release branches** (e.g. `main`).<br/>
Other branches are free to play and run experiments as needed.

## References:

- [.NET SDK and Tools](https://learn.microsoft.com/dotnet/navigate/tools-diagnostics/)
