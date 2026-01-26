// RejectTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.Reject;

public class RejectTests
{
  [Test]
  public async Task Reject_moves_pr_to_rejected()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "pending"));
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "rejected"));

    try
    {
      var doc = new PrDoc
      {
        Id = "pr-005",
        CanonicalUri = "pr://pr-005",
        Title = "Title",
        Summary = "Summary",
        Kind = "atomic",
        Status = "pending",
        CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
        Author = "tester",
        Base = new PrBranch { Branch = "default" },
        Head = new PrBranch { Branch = "feature/pr-005" },
        RealmsTouched = new List<string> { "realm://monocoque.tools" },
        TurnRefs = new List<string>(),
        Checks = new List<PrCheck>(),
        Review = new PrReview()
      };

      PrDocStore.Save(root, PrPaths.BuildPendingRel(doc.Id), doc);

      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = RejectLogic.Run(new RejectOptions("pr://pr-005", "not needed"), context);

      await Assert.That(result.Ok).IsTrue();
      await Assert.That(File.Exists(Path.Combine(root, "[prs]", "rejected", "pr-005.pr.yaml"))).IsTrue();
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
