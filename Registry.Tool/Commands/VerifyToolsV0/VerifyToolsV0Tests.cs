// VerifyToolsV0Tests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Registry.Tool.App.Logging;
using Registry.Tool.Commands.BootstrapToolsV0;

namespace Registry.Tool.Commands.VerifyToolsV0;

public class VerifyToolsV0Tests
{
  [Test]
  public async Task Verify_succeeds_after_bootstrap()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      var bootstrap = BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      await Assert.That(bootstrap.Ok).IsTrue();

      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsTrue();
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Missing_schema_file_fails()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      var toolSchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool.schema.json");
      File.Delete(toolSchemaPath);

      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("missing [monocoque.tools]/schemas/v0/tool.schema.json");
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Invalid_registry_yaml_fails()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      var registryPath = Path.Combine(root, "[monocoque.tools]", "registry", "v0", "registry.yaml");
      File.WriteAllText(registryPath, ":not-yaml");

      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("invalid YAML in [monocoque.tools]/registry/v0/registry.yaml");
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Missing_tool_doc_fails()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");
      File.Delete(toolDocPath);

      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("missing tool doc");
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Tool_doc_schema_violation_fails()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");
      File.WriteAllText(toolDocPath, "schema: tool.v0\n");

      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("schema validation failed");
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Command_tests_count_invariant_fails()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.ur]"));

    try
    {
      WriteTemplatePack(root);
      WriteAuthorityMap(root);
      WriteCommandLayout(root);

      var extraTestPath = Path.Combine(root, "[monocoque.tools]", "Registry.Tool", "Commands", "BootstrapToolsV0", "ExtraTests.cs");
      File.WriteAllText(extraTestPath, "// extra test");

      BootstrapToolsV0Logic.Run(new BootstrapOptions(root, false, new LogOptions(null, null)));
      var verify = VerifyToolsV0Logic.Run(new VerifyOptions(root, new LogOptions(null, null)));
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("expected exactly one *Tests.cs");
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

  private static void WriteAuthorityMap(string root)
  {
    File.WriteAllText(Path.Combine(root, "AUTHORITY-MAP.v0.md"),
      "Authority map: [monocoque.ur]/registry/v0/authority-map.v0.yaml\n");

    var mapDir = Path.Combine(root, "[monocoque.ur]", "registry", "v0");
    Directory.CreateDirectory(mapDir);
    File.WriteAllText(Path.Combine(mapDir, "authority-map.v0.yaml"),
      "schema: authority_map.v0\n"
      + "registries:\n"
      + "  - realm: tools\n"
      + "    registry_path_rel: \"[monocoque.tools]/registry/v0/registry.yaml\"\n");

    var schemaDir = Path.Combine(root, "[monocoque.ur]", "schemas", "v0");
    Directory.CreateDirectory(schemaDir);
    File.WriteAllText(Path.Combine(schemaDir, "authority_map.schema.json"), AuthorityMapSchemaTemplate());
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

  private static void WriteCommandLayout(string root)
  {
    var commandsRoot = Path.Combine(root, "[monocoque.tools]", "Registry.Tool", "Commands");
    WriteCommandDir(commandsRoot, "BootstrapToolsV0");
    WriteCommandDir(commandsRoot, "VerifyToolsV0");
    WriteCommandDir(commandsRoot, "Registry");
  }

  private static void WriteCommandDir(string commandsRoot, string name)
  {
    var dir = Path.Combine(commandsRoot, name);
    Directory.CreateDirectory(dir);
    File.WriteAllText(Path.Combine(dir, name + "Command.cs"), "// command\n");
    File.WriteAllText(Path.Combine(dir, name + "Tests.cs"), "// tests\n");
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

  private static string AuthorityMapSchemaTemplate()
  {
    return "{\n"
      + "  \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n"
      + "  \"$id\": \"authority_map.schema.v0\",\n"
      + "  \"title\": \"Authority map schema v0\",\n"
      + "  \"type\": \"object\",\n"
      + "  \"additionalProperties\": false,\n"
      + "  \"required\": [\"schema\", \"registries\"],\n"
      + "  \"properties\": {\n"
      + "    \"schema\": { \"const\": \"authority_map.v0\" },\n"
      + "    \"registries\": {\n"
      + "      \"type\": \"array\",\n"
      + "      \"minItems\": 1,\n"
      + "      \"items\": {\n"
      + "        \"type\": \"object\",\n"
      + "        \"additionalProperties\": false,\n"
      + "        \"required\": [\"realm\", \"registry_path_rel\"],\n"
      + "        \"properties\": {\n"
      + "          \"realm\": { \"type\": \"string\", \"minLength\": 1 },\n"
      + "          \"registry_path_rel\": { \"type\": \"string\", \"pattern\": \"^\\\\[[^\\\\]]+\\\\]/\" },\n"
      + "          \"notes\": { \"type\": \"string\" }\n"
      + "        }\n"
      + "      }\n"
      + "    }\n"
      + "  }\n"
      + "}\n";
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
