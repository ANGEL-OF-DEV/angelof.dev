// StandardArgumentException.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using angelof.dev.Internals;

namespace angelof.dev.Exceptions;

public sealed class StandardArgumentException : ArgumentOutOfRangeException
{
  public StandardArgumentException(object? actualValue,
                                   string argumentName,
                                   string expectedValue,
                                   string callerMemberName,
                                   string callerFilePath,
                                   int callerLineNumber)
    : base(argumentName, actualValue, "Argument value is not expected and invalid.")
  {
    ExpectedValue = expectedValue;
  }

  public string ExpectedValue { get; }

  public ReadOnlySpan<char> StableMessage => GetStableMessageSpan();

  private ReadOnlySpan<char> GetStableMessageSpan()
  {
    var span = Message.AsSpan();
    var lineEndIndex = span.IndexOfAny(ReusableSearchValues.LineEndSearchValues);
    return lineEndIndex == -1 ? span : span[..lineEndIndex];
  }
}
