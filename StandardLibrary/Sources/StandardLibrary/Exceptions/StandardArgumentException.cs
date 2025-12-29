// StandardArgumentException.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using angelof.dev.Internals;

namespace angelof.dev.Exceptions;

/// <summary> Represents an exception thrown when method argument is out of expected range or otherwise invalid. </summary>
public sealed class StandardArgumentException : ArgumentOutOfRangeException
{
  /// <summary> Initializes a new instance. </summary>
  public StandardArgumentException(object? actualValue,
                                   string argumentName,
                                   string expectedValue,
                                   string sourceMemberName,
                                   string sourceFilePath,
                                   int sourceLineNumber)
    : base(argumentName, actualValue, "Argument value is not expected and invalid.")
  {
    ExpectedValue = expectedValue;
    SourceMemberName = sourceMemberName;
    SourceFilePath = sourceFilePath;
    SourceLineNumber = sourceLineNumber;
  }

  public string ExpectedValue { get; }

  public string SourceMemberName { get; }

  public string SourceFilePath { get; }

  public int SourceLineNumber { get; }

  public ReadOnlySpan<char> StableMessage => GetStableMessageSpan();

  private ReadOnlySpan<char> GetStableMessageSpan()
  {
    var span = Message.AsSpan();
    var lineEndIndex = span.IndexOfAny(ReusableSearchValues.LineEndSearchValues);
    return lineEndIndex == -1 ? span : span[..lineEndIndex];
  }
}
