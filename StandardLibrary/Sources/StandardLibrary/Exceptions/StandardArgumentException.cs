// StandardArgumentException.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using angelof.dev.Internals;

namespace angelof.dev.Exceptions;

public sealed class StandardArgumentException : ArgumentOutOfRangeException
{
  private const string ArgumentActualValueString = "ArgumentActualValue";

  public StandardArgumentException(object? actualValue,
                                   string argumentName,
                                   string expectedValue,
                                   string callerMemberName,
                                   string callerFilePath,
                                   int callerLineNumber)
    : base(argumentName, actualValue, "Argument value is not expected and invalid.")
  {
    ExpectedValue = expectedValue;
    Data.Add(ArgumentActualValueString, actualValue);
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
