using Ur.Tool.Commands.Verify;

namespace Ur.Tool.Tests;

public class VerifyMdFrontmatterLogicTests
{
  [Test]
  public async Task Fails_when_ur_md_missing_frontmatter()
  {
    var root = Path.Combine(Path.GetTempPath(), "urtool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);

    try
    {
      File.WriteAllText(Path.Combine(root, "bad.ur.md"), "# no frontmatter");

      var result = VerifyMdFrontmatterLogic.Run(root);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(result.Errors.Count).IsGreaterThan(0);
    }
    finally
    {
      if (Directory.Exists(root))
        Directory.Delete(root, recursive: true);
    }
  }
}
