// Program.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using angelof.dev.ffrwd.Commands;
using Spectre.Console.Cli;

namespace angelof.dev.ffrwd;

internal static class Program
{
  public static int Main(string[] args)
  {
    var app = new CommandApp();

    app.Configure(config =>
    {
      config.SetApplicationName("ffrwd");
      config.CaseSensitivity(CaseSensitivity.None);

      config.AddBranch("identity",
                       identity =>
                       {
                         identity.SetDescription("Identity commands.");
                         identity.AddCommand<IdentityGetCommand>("get")
                                 .WithDescription("Get identity string for a model.");
                       });

      config.AddBranch("agent",
                       agent =>
                       {
                         agent.SetDescription("Agent commands.");
                         agent.AddCommand<AgentInitCommand>("init")
                              .WithDescription("Initialize agent worktree.");
                         agent.AddCommand<AgentStartCommand>("start")
                              .WithDescription("Initialize worktree and load agent instructions.");
                         agent.AddCommand<AgentDoctrineCommand>("doctrine")
                              .WithDescription("Emit doctrine file manifest.");
                         agent.AddBranch("task",
                                         task =>
                                         {
                                           task.SetDescription("Agent tasking commands.");
                                           task.AddCommand<AgentTaskNextCommand>("next")
                                               .WithDescription("Get next task for agent.");
                                         });
                       });

      config.AddBranch("frontmatter",
                       frontmatter =>
                       {
                         frontmatter.SetDescription("Frontmatter commands.");
                         frontmatter.AddCommand<FrontmatterExtractCommand>("extract")
                                    .WithDescription("Extract frontmatter JSON from a .yml.md file.");
                         frontmatter.AddCommand<FrontmatterExtractAllCommand>("extract-all")
                                    .WithDescription("Extract frontmatter JSON from all .yml.md files.");
                       });
    });

    return app.Run(args);
  }
}
