// ListModels.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using Pr.Tool.App.Infrastructure;

namespace Pr.Tool.Commands.List;

public sealed record ListOptions();

public sealed record ListResult(CommandResult Result, IReadOnlyList<string> Lines);
