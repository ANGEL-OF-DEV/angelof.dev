// BootstrapToolsV0Tests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Registry.Tool.App.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Registry.Tool.Commands.BootstrapToolsV0;

public class BootstrapToolsV0Tests
{
  [Test]
  public async Task Bootstrap_is_idempotent()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);

      var first = BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      await Assert.That(first.Ok).IsTrue();

      var registryPath = Path.Combine(root, "[monocoque.tools]", "registry", "v0", "registry.yaml");
      var toolSchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool.schema.json");
      var toolRegistrySchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool_registry.schema.json");
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");

      var registryBefore = File.ReadAllText(registryPath);
      var toolSchemaBefore = File.ReadAllText(toolSchemaPath);
      var toolRegistrySchemaBefore = File.ReadAllText(toolRegistrySchemaPath);
      var toolDocBefore = File.ReadAllText(toolDocPath);

      var second = BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      await Assert.That(second.Ok).IsTrue();

      var registryAfter = File.ReadAllText(registryPath);
      var toolSchemaAfter = File.ReadAllText(toolSchemaPath);
      var toolRegistrySchemaAfter = File.ReadAllText(toolRegistrySchemaPath);
      var toolDocAfter = File.ReadAllText(toolDocPath);

      await Assert.That(registryAfter).IsEqualTo(registryBefore);
      await Assert.That(toolSchemaAfter).IsEqualTo(toolSchemaBefore);
      await Assert.That(toolRegistrySchemaAfter).IsEqualTo(toolRegistrySchemaBefore);
      await Assert.That(toolDocAfter).IsEqualTo(toolDocBefore);

      var registry = DeserializeYaml<ToolRegistryDoc>(registryAfter);
      await Assert.That(registry.Tools.Count).IsEqualTo(1);
    }
    finally
    {
      Cleanup(root);
    }
  }

  private static string CreateTempRoot()
  {
    var root = Path.Combine(Path.GetTempPath(), "registry-tool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);
    return root;
  }

  private static void Cleanup(string root)
  {
    if (Directory.Exists(root))
      Directory.Delete(root, recursive: true);
  }

  private static void WriteTemplatePack(string root)
  {
    var templateRoot = Path.Combine(root, "[monocoque.ur]", "templates", "registry-tool", "v0");
    Directory.CreateDirectory(templateRoot);

    File.WriteAllText(Path.Combine(templateRoot, "tool.schema.json"), ToolSchemaTemplate());
    File.WriteAllText(Path.Combine(templateRoot, "tool_registry.schema.json"), ToolRegistrySchemaTemplate());
    File.WriteAllText(Path.Combine(templateRoot, "tool.doc.yaml"), ToolDocTemplate());
    File.WriteAllText(Path.Combine(templateRoot, "tool.registry.yaml"), ToolRegistryTemplate());
    File.WriteAllText(Path.Combine(templateRoot, "pack.yaml"), PackTemplate());
  }

  private static T DeserializeYaml<T>(string yaml) where T : class
  {
    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(NullNamingConvention.Instance)
      .Build();

    return deserializer.Deserialize<T>(yaml);
  }

  private static string PackTemplate()
  {
    return "schema: template_pack.v0\n"
      + "id: registry-tool.v0\n"
      + "title: Registry.Tool template pack v0\n"
      + "templates:\n"
      + "  - id: tool.schema.json\n"
      + "    path: \"[monocoque.ur]/templates/registry-tool/v0/tool.schema.json\"\n"
      + "  - id: tool_registry.schema.json\n"
      + "    path: \"[monocoque.ur]/templates/registry-tool/v0/tool_registry.schema.json\"\n"
      + "  - id: tool.doc.yaml\n"
      + "    path: \"[monocoque.ur]/templates/registry-tool/v0/tool.doc.yaml\"\n"
      + "  - id: tool.registry.yaml\n"
      + "    path: \"[monocoque.ur]/templates/registry-tool/v0/tool.registry.yaml\"\n"
      + "placeholders:\n"
      + "  - \"MONOCOQUE_PLACEHOLDER(V0): remove bootstrap hardcode; resolve templates via registry only\"\n";
  }

  private static string ToolSchemaTemplate()
  {
    return "{\n"
      + "  \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n"
      + "  \"$id\": \"tool.schema.v0\",\n"
      + "  \"title\": \"Tool schema v0\",\n"
      + "  \"type\": \"object\",\n"
      + "  \"additionalProperties\": false,\n"
      + "  \"required\": [\"schema\", \"id\", \"canonical_path\", \"project_path_rel\", \"kind\", \"commands\", \"placeholders\"],\n"
      + "  \"properties\": {\n"
      + "    \"schema\": { \"const\": \"tool.v0\" },\n"
      + "    \"id\": { \"type\": \"string\", \"minLength\": 1 },\n"
      + "    \"canonical_path\": { \"type\": \"string\", \"pattern\": \"^tool://\" },\n"
      + "    \"project_path_rel\": { \"type\": \"string\", \"pattern\": \"^\\\\[[^\\\\]]+\\\\]/\" },\n"
      + "    \"kind\": { \"type\": \"string\", \"enum\": [\"dotnet\"] },\n"
      + "    \"commands\": {\n"
      + "      \"type\": \"array\",\n"
      + "      \"minItems\": 1,\n"
      + "      \"items\": {\n"
      + "        \"type\": \"object\",\n"
      + "        \"additionalProperties\": false,\n"
      + "        \"required\": [\"name\", \"example\"],\n"
      + "        \"properties\": {\n"
      + "          \"name\": { \"type\": \"string\", \"minLength\": 1 },\n"
      + "          \"example\": { \"type\": \"string\", \"minLength\": 1 }\n"
      + "        }\n"
      + "      }\n"
      + "    },\n"
      + "    \"generated_at_utc\": { \"type\": \"string\", \"format\": \"date-time\" },\n"
      + "    \"placeholders\": {\n"
      + "      \"type\": \"array\",\n"
      + "      \"minItems\": 1,\n"
      + "      \"items\": { \"type\": \"string\" }\n"
      + "    }\n"
      + "  }\n"
      + "}\n";
  }

  private static string ToolRegistrySchemaTemplate()
  {
    return "{\n"
      + "  \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n"
      + "  \"$id\": \"tool_registry.schema.v0\",\n"
      + "  \"title\": \"Tool registry schema v0\",\n"
      + "  \"type\": \"object\",\n"
      + "  \"additionalProperties\": false,\n"
      + "  \"required\": [\"schema\", \"canonical_path\", \"tools\", \"placeholders\"],\n"
      + "  \"properties\": {\n"
      + "    \"schema\": { \"const\": \"tool_registry.v0\" },\n"
      + "    \"canonical_path\": { \"type\": \"string\", \"pattern\": \"^tool-registry://\" },\n"
      + "    \"tools\": {\n"
      + "      \"type\": \"array\",\n"
      + "      \"minItems\": 1,\n"
      + "      \"items\": {\n"
      + "        \"type\": \"object\",\n"
      + "        \"additionalProperties\": false,\n"
      + "        \"required\": [\"id\", \"doc_path_rel\", \"status\"],\n"
      + "        \"properties\": {\n"
      + "          \"id\": { \"type\": \"string\", \"minLength\": 1 },\n"
      + "          \"doc_path_rel\": { \"type\": \"string\", \"pattern\": \"^\\\\[[^\\\\]]+\\\\]/\" },\n"
      + "          \"status\": { \"type\": \"string\", \"enum\": [\"active\", \"deprecated\", \"inactive\"] },\n"
      + "          \"notes\": { \"type\": \"string\" }\n"
      + "        }\n"
      + "      }\n"
      + "    },\n"
      + "    \"generated_at_utc\": { \"type\": \"string\", \"format\": \"date-time\" },\n"
      + "    \"placeholders\": {\n"
      + "      \"type\": \"array\",\n"
      + "      \"minItems\": 1,\n"
      + "      \"items\": { \"type\": \"string\" }\n"
      + "    }\n"
      + "  }\n"
      + "}\n";
  }

  private static string ToolDocTemplate()
  {
    return "schema: tool.v0\n"
      + "id: \"{{TOOL_ID}}\"\n"
      + "canonical_path: \"{{TOOL_CANONICAL_PATH}}\"\n"
      + "project_path_rel: \"{{TOOL_PROJECT_PATH_REL}}\"\n"
      + "kind: dotnet\n"
      + "commands:\n"
      + "  - name: bootstrap-tools-v0\n"
      + "    example: \"{{BOOTSTRAP_COMMAND}}\"\n"
      + "  - name: verify-tools-v0\n"
      + "    example: \"{{VERIFY_COMMAND}}\"\n"
      + "placeholders:\n"
      + "  - \"MONOCOQUE_PLACEHOLDER(V0): metrics emission contract\"\n";
  }

  private static string ToolRegistryTemplate()
  {
    return "schema: tool_registry.v0\n"
      + "canonical_path: tool-registry://tools.v0\n"
      + "tools: []\n"
      + "placeholders:\n"
      + "  - \"MONOCOQUE_PLACEHOLDER(V0): metrics emission contract\"\n";
  }
}
