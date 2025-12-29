// StandardExpectationException.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.Exceptions;

/// <summary> Represents an exception thrown when run-time expectation is not met. </summary>
public sealed class StandardExpectationException : InvalidOperationException
{
  /// <summary> Initializes a new instance. </summary>
  public StandardExpectationException(string expectation,
                                      string expectedConditionExpression,
                                      string sourceMemberName,
                                      string sourceFilePath,
                                      int sourceLineNumber)
  {
    Expectation = expectation;
    ExpectedConditionExpression = expectedConditionExpression;
    SourceMemberName = sourceMemberName;
    SourceFilePath = sourceFilePath;
    SourceLineNumber = sourceLineNumber;
  }

  public string SourceMemberName { get; }
  public string SourceFilePath { get; }
  public int SourceLineNumber { get; }
  public string Expectation { get; }
  public string ExpectedConditionExpression { get; }
}
