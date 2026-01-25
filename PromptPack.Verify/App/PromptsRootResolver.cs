// PromptsRootResolver.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
namespace PromptPack.Verify.App;

public static class PromptsRootResolver
{
  public static string Resolve(string? promptsRootOpt, string? packRootOpt)
  {
    if (!string.IsNullOrWhiteSpace(promptsRootOpt))
      return Normalize(promptsRootOpt);

    if (!string.IsNullOrWhiteSpace(packRootOpt))
      return Normalize(packRootOpt);

    var envRoot = Environment.GetEnvironmentVariable("MONOCOQUE_PROMPTS_ROOT");
    if (!string.IsNullOrWhiteSpace(envRoot))
      return Normalize(envRoot);

    var cwd = Directory.GetCurrentDirectory();
    var sibling = Path.GetFullPath(Path.Combine(cwd, "..", "[monocoque.prompts]"));
    if (Directory.Exists(Path.Combine(sibling, "registry")))
      return sibling;

    return Normalize(cwd);
  }

  public static string Normalize(string root)
  {
    var full = Path.GetFullPath(root);
    var legacy = Path.Combine(full, "[prompts]");
    if (Directory.Exists(Path.Combine(legacy, "registry")))
      return legacy;

    return full;
  }
}
