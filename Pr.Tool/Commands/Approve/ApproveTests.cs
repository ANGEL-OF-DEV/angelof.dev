// ApproveTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.Linq;
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.Approve;

public class ApproveTests
{
  [Test]
  public async Task Approve_requires_default_branch()
  {
    var root = CreateTempRoot();

    try
    {
      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/feature\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "feature", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = ApproveLogic.Run(new ApproveOptions("pr://missing", "tester", false, false, false), context);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(result.Errors.Any(e => e.Contains("default"))).IsTrue();
    }
    finally
    {
      Cleanup(root);
    }
  }

  [Test]
  public async Task Approve_requires_clean_status()
  {
    var root = CreateTempRoot();

    try
    {
      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, " M file", string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = ApproveLogic.Run(new ApproveOptions("pr://missing", "tester", false, false, false), context);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(result.Errors.Any(e => e.Contains("clean"))).IsTrue();
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
