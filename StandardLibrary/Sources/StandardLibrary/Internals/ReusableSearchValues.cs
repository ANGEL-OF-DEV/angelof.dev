// ReusableSearchValues.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Buffers;

namespace angelof.dev.Internals;

internal static class ReusableSearchValues
{
  public static readonly SearchValues<char> LineEndSearchValues = SearchValues.Create('\r', '\n');
}
