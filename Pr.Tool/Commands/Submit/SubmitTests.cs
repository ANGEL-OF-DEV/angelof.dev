// SubmitTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.Submit;

public class SubmitTests
{
  [Test]
  public async Task Submit_moves_draft_to_pending()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "draft"));
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "pending"));

    try
    {
      var doc = new PrDoc
      {
        Id = "pr-002",
        CanonicalUri = "pr://pr-002",
        Title = "Title",
        Summary = "Summary",
        Kind = "atomic",
        Status = "draft",
        CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
        Author = "tester",
        Base = new PrBranch { Branch = "default" },
        Head = new PrBranch { Branch = "feature/pr-002" },
        RealmsTouched = new List<string> { "realm://monocoque.tools" },
        TurnRefs = new List<string>(),
        Checks = new List<PrCheck>(),
        Review = new PrReview()
      };

      PrDocStore.Save(root, PrPaths.BuildDraftRel(doc.Id), doc);

      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));
      git.Add(root, new[] { "show-ref", "--verify", "refs/heads/default" }, new GitRunResult(0, "", string.Empty));
      git.Add(root, new[] { "show-ref", "--verify", "refs/heads/feature/pr-002" }, new GitRunResult(0, "", string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = SubmitLogic.Run(new SubmitOptions("pr-002"), context);

      await Assert.That(result.Ok).IsTrue();
      await Assert.That(File.Exists(Path.Combine(root, "[prs]", "pending", "pr-002.pr.yaml"))).IsTrue();
      await Assert.That(File.Exists(Path.Combine(root, "[prs]", "draft", "pr-002.pr.yaml"))).IsFalse();
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
