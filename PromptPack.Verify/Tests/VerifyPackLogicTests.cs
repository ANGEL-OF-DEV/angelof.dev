namespace PromptPack.Verify.Tests;

public class VerifyPackLogicTests
{
  [Test]
  public async Task Fails_when_pack_missing_files()
  {
    var root = Path.Combine(Path.GetTempPath(), "promptpack-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);

    try
    {
      var result = PromptPack.Verify.App.VerifyPackLogic.Run(root);

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
