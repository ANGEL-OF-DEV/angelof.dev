// SuggestionEngine.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
using System;
using System.Collections.Generic;
using System.Linq;

namespace Forks.Tool.Services
{
  public static class SuggestionEngine
  {
    public static IReadOnlyList<ForkRecord> Suggest(IReadOnlyList<string> packageIds, IReadOnlyList<ForkRecord> existing)
    {
      var existingSet = existing.ToDictionary(x => x.Package, StringComparer.OrdinalIgnoreCase);
      var suggestions = new List<ForkRecord>();

      foreach (var id in packageIds)
      {
        if (existingSet.ContainsKey(id))
        {
          continue;
        }

        suggestions.Add(new ForkRecord(id, "nuget", "", "Missing fork entry", null, null));
      }

      return suggestions;
    }
  }
}
