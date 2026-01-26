// ListTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.List;

public class ListTests
{
  [Test]
  public async Task List_reads_pending_index_when_present()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "pending"));

    try
    {
      var index = new PendingIndex
      {
        UpdatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
        Pending = new List<PendingEntry>
        {
          new PendingEntry
          {
            PrUri = "pr://pr-003",
            PrFileRepoUri = "file.repo://[prs]/pending/pr-003.pr.yaml",
            Title = "Title",
            Kind = "atomic",
            CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O"),
            RealmsTouched = new List<string> { "realm://monocoque.tools" }
          }
        }
      };

      File.WriteAllText(Path.Combine(root, PrPaths.PendingIndexRel), YamlHelpers.Serialize(index));

      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = ListLogic.Run(new ListOptions(), context);

      await Assert.That(result.Result.Ok).IsTrue();
      await Assert.That(result.Lines.Count).IsEqualTo(1);
      await Assert.That(result.Lines[0].Contains("pr://pr-003")).IsTrue();
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
