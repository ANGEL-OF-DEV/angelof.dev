// BootstrapToolsV0Tests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Text.Json;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public class BootstrapToolsV0Tests
{
  [Test]
  public async Task Bootstrap_is_idempotent()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));

    try
    {
      var first = BootstrapToolsV0Logic.Run(root);
      await Assert.That(first.Ok).IsTrue();

      var registryPath = Path.Combine(root, "[monocoque.tools]", "registry", "v0", "registry.yaml");
      var toolSchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool.schema.json");
      var toolRegistrySchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool_registry.schema.json");
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");

      var registryBefore = File.ReadAllText(registryPath);
      var toolSchemaBefore = File.ReadAllText(toolSchemaPath);
      var toolRegistrySchemaBefore = File.ReadAllText(toolRegistrySchemaPath);
      var toolDocBefore = File.ReadAllText(toolDocPath);

      var second = BootstrapToolsV0Logic.Run(root);
      await Assert.That(second.Ok).IsTrue();

      var registryAfter = File.ReadAllText(registryPath);
      var toolSchemaAfter = File.ReadAllText(toolSchemaPath);
      var toolRegistrySchemaAfter = File.ReadAllText(toolRegistrySchemaPath);
      var toolDocAfter = File.ReadAllText(toolDocPath);

      await Assert.That(registryAfter).IsEqualTo(registryBefore);
      await Assert.That(toolSchemaAfter).IsEqualTo(toolSchemaBefore);
      await Assert.That(toolRegistrySchemaAfter).IsEqualTo(toolRegistrySchemaBefore);
      await Assert.That(toolDocAfter).IsEqualTo(toolDocBefore);

      using var doc = JsonDocument.Parse(registryAfter);
      var tools = doc.RootElement.GetProperty("tools");
      await Assert.That(tools.GetArrayLength()).IsEqualTo(1);
    }
    finally
    {
      if (Directory.Exists(root))
        Directory.Delete(root, recursive: true);
    }
  }

  private static string CreateTempRoot()
  {
    var root = Path.Combine(Path.GetTempPath(), "registry-tool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);
    return root;
  }
}
