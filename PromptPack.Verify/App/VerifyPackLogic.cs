namespace PromptPack.Verify.App;

public sealed record VerifyResult(bool Ok, IReadOnlyList<string> Errors);

public static class VerifyPackLogic
{
  public static VerifyResult Run(string packRoot)
  {
    var errors = new List<string>();
    var root = Path.GetFullPath(packRoot);

    var required = new[]
    {
      Path.Combine(root, "[prompts]", "registry", "v0", "prompt-registry.yaml"),
      Path.Combine(root, "[prompts]", "prompts", "angelofdev-copilots-general-v0.txt"),
      Path.Combine(root, "[prompts]", "prompts", "angelofdev-copilots-orchestrated-turn-workflow-v0.txt"),
      Path.Combine(root, "[prompts]", "logs", "CHANGELOG.txt"),
      Path.Combine(root, "[prompts]", "logs", "DECISIONLOG.txt")
    };

    foreach (var p in required)
      if (!File.Exists(p))
        errors.Add($"Missing required file: {p}");

    // Validate minimal headers in prompt specs
    foreach (var prompt in required.Where(p => p.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
    {
      if (!File.Exists(prompt)) continue;

      var text = File.ReadAllText(prompt);
      RequireContains(errors, prompt, text, "# ID:");
      RequireContains(errors, prompt, text, "# CNAME:");
      RequireContains(errors, prompt, text, "# VERSION:");
    }

    return new VerifyResult(errors.Count == 0, errors);
  }

  private static void RequireContains(List<string> errors, string file, string text, string needle)
  {
    if (!text.Contains(needle, StringComparison.Ordinal))
      errors.Add($"{file}: missing required header fragment '{needle}'");
  }
}
