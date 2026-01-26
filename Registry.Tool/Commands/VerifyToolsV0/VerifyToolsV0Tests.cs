// VerifyToolsV0Tests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Registry.Tool.Commands.BootstrapToolsV0;

namespace Registry.Tool.Commands.VerifyToolsV0;

public class VerifyToolsV0Tests
{
  [Test]
  public async Task Verify_succeeds_after_bootstrap()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[monocoque.tools]"));

    try
    {
      var bootstrap = BootstrapToolsV0Logic.Run(root);
      await Assert.That(bootstrap.Ok).IsTrue();

      var verify = VerifyToolsV0Logic.Run(root);
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

    try
    {
      BootstrapToolsV0Logic.Run(root);
      var toolSchemaPath = Path.Combine(root, "[monocoque.tools]", "schemas", "v0", "tool.schema.json");
      File.Delete(toolSchemaPath);

      var verify = VerifyToolsV0Logic.Run(root);
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("missing tool.schema.json");
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

    try
    {
      BootstrapToolsV0Logic.Run(root);
      var registryPath = Path.Combine(root, "[monocoque.tools]", "registry", "v0", "registry.yaml");
      File.WriteAllText(registryPath, "not-json");

      var verify = VerifyToolsV0Logic.Run(root);
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("invalid JSON");
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

    try
    {
      BootstrapToolsV0Logic.Run(root);
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");
      File.Delete(toolDocPath);

      var verify = VerifyToolsV0Logic.Run(root);
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

    try
    {
      BootstrapToolsV0Logic.Run(root);
      var toolDocPath = Path.Combine(root, "[monocoque.tools]", "tools", "v0", "registry.tool.yaml");
      File.WriteAllText(toolDocPath, "{\"schema\":null}");

      var verify = VerifyToolsV0Logic.Run(root);
      await Assert.That(verify.Ok).IsFalse();
      await Assert.That(string.Join("\n", verify.Errors)).Contains("'schema' must be a string");
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
}
