// CreateTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.Create;

public class CreateTests
{
  [Test]
  public async Task Create_pending_updates_index()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "pending"));
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "draft"));

    try
    {
      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));

      var options = new CreateOptions(
        "pr-001",
        "Title",
        "Summary",
        "default",
        "feature/pr-001",
        "pending",
        new[] { "realm://monocoque.tools" },
        Array.Empty<string>());

      var context = new CommandContext(root, git, new LogOptions(null, null),
        () => DateTimeOffset.Parse("2026-01-26T00:00:00Z"));

      var result = CreateLogic.Run(options, context);
      await Assert.That(result.Ok).IsTrue();

      var prPath = Path.Combine(root, "[prs]", "pending", "pr-001.pr.yaml");
      await Assert.That(File.Exists(prPath)).IsTrue();

      var indexPath = Path.Combine(root, PrPaths.PendingIndexRel);
      await Assert.That(File.Exists(indexPath)).IsTrue();

      var errors = new List<string>();
      var index = YamlHelpers.Deserialize<PendingIndex>(File.ReadAllText(indexPath), "index", errors);
      await Assert.That(errors.Count).IsEqualTo(0);
      await Assert.That(index?.Pending.Count ?? 0).IsEqualTo(1);
    }
    finally
    {
      Cleanup(root);
    }
  }

  private static string CreateTempRoot()
  {
    var root = Path.Combine(Path.GetTempPath(), "pr-tool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);
    return root;
  }

  private static void Cleanup(string root)
  {
    if (Directory.Exists(root))
      Directory.Delete(root, recursive: true);
  }
}
