// RegistryCommandFactoryTests.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System.CommandLine;
using System.Linq;

namespace Registry.Tool.Commands.Registry;

public class RegistryCommandFactoryTests
{
  [Test]
  public async Task Registry_command_includes_expected_subcommands()
  {
    var cmd = RegistryCommandFactory.Create();
    var names = cmd.Subcommands.Select(sub => sub.Name).ToList();

    await Assert.That(names).Contains("bootstrap-tools-v0");
    await Assert.That(names).Contains("verify-tools-v0");
    await Assert.That(names).Contains("help");
  }
}
