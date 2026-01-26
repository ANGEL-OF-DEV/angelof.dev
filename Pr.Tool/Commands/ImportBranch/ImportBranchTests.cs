// ImportBranchTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;
using Pr.Tool.App.Logging;
using Pr.Tool.TestSupport;

namespace Pr.Tool.Commands.ImportBranch;

public class ImportBranchTests
{
  [Test]
  public async Task Import_branch_derives_id_from_branch()
  {
    var root = CreateTempRoot();
    Directory.CreateDirectory(Path.Combine(root, "[prs]", "draft"));

    try
    {
      var git = new FakeGitRunner();
      git.Add(root, new[] { "worktree", "list", "--porcelain" },
        new GitRunResult(0, $"worktree {root}\nHEAD abc\nbranch refs/heads/default\n", string.Empty));
      git.Add(root, new[] { "branch", "--show-current" }, new GitRunResult(0, "default", string.Empty));
      git.Add(root, new[] { "status", "--porcelain" }, new GitRunResult(0, string.Empty, string.Empty));
      git.Add(root, new[] { "diff", "--name-only", "default...tools/20260126T120000Z-sample" },
        new GitRunResult(0, "[monocoque.tools]/README.md", string.Empty));

      var options = new ImportBranchOptions(
        "tools/20260126T120000Z-sample",
        "default",
        "draft",
        false,
        Array.Empty<string>());

      var context = new CommandContext(root, git, new LogOptions(null, null), () => DateTimeOffset.UtcNow);
      var result = ImportBranchLogic.Run(options, context);

      await Assert.That(result.Ok).IsTrue();
      await Assert.That(File.Exists(Path.Combine(root, "[prs]", "draft", "tools-20260126T120000Z-sample.pr.yaml"))).IsTrue();
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
