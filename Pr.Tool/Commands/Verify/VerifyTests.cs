// VerifyTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.Verify;

public class VerifyTests
{
  [Test]
  public async Task Verify_rejects_multi_realm_atomic_by_default()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "draft"));

    try
    {
      var doc = new PrDoc
      {
        Id = "pr-004",
        CanonicalUri = "pr://pr-004",
        Title = "Title",
        Summary = "Summary",
        Kind = "atomic",
        Status = "draft",
        CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
        Author = "tester",
        Base = new PrBranch { Branch = "default" },
        Head = new PrBranch { Branch = "feature/pr-004" },
        RealmsTouched = new List<string> { "realm://monocoque.tools", "realm://monocoque.ur" },
        TurnRefs = new List<string>(),
        Checks = new List<PrCheck>(),
        Review = new PrReview()
      };

      var rel = PrPaths.BuildDraftRel(doc.Id);
      PrDocStore.Save(root, rel, doc);

      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "show-ref", "--verify", "refs/heads/default" }, new GitRunResult(0, "", string.Empty));
      git.Add(root, new[] { "show-ref", "--verify", "refs/heads/feature/pr-004" }, new GitRunResult(0, "", string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = VerifyLogic.Run(new VerifyOptions(rel, false), context);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(result.Errors.Count).IsGreaterThan(0);
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
