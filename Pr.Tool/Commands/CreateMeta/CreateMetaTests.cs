// CreateMetaTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.CreateMeta;

public class CreateMetaTests
{
  [Test]
  public async Task Create_meta_applies_merge_order_override()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "draft"));
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "pending"));

    try
    {
      var child = new PrDoc
      {
        Id = "child-1",
        CanonicalUri = "pr://child-1",
        Title = "Child",
        Summary = "Child",
        Kind = "atomic",
        Status = "draft",
        CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
        Author = "tester",
        Base = new PrBranch { Branch = "default" },
        Head = new PrBranch { Branch = "feature/child" },
        RealmsTouched = new List<string> { "realm://monocoque.tools" },
        TurnRefs = new List<string>(),
        Checks = new List<PrCheck>(),
        Review = new PrReview()
      };

      PrDocStore.Save(root, PrPaths.BuildDraftRel(child.Id), child);

      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));

      var options = new CreateMetaOptions(
        "meta-1",
        "Meta",
        "Meta",
        new[] { "pr://child-1" },
        "draft",
        "ur,tools,root");

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = CreateMetaLogic.Run(options, context);

      await Assert.That(result.Ok).IsTrue();

      var rel = PrPaths.BuildDraftRel("meta-1");
      var loaded = PrDocStore.Load(root, rel, new List<string>());
      await Assert.That(loaded?.MergeOrder?.Count ?? 0).IsEqualTo(3);
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
