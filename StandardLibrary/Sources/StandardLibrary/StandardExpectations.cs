// StandardExpectations.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.Exceptions;

namespace angelof.dev;

/// <summary> Provides methods for validating run-time expectations. </summary>
/// <remarks>
///   All methods throw <see cref="InvalidOperationException" /> derived exception when their expectation check
///   fails.
/// </remarks>
public static class StandardExpectations
{
  /// <summary> Throws when object is <b> null </b>. </summary>
  /// <exception cref="StandardExpectationException"> <paramref name="obj" /> is <b> null </b> </exception>
  [return: NotNullIfNotNull(nameof(obj))]
  public static T ExpectNotNull<T>(T? obj,
                                   [CAE(nameof(obj))] string expression = "",
                                   [CMN] string callerMemberName = "",
                                   [CFP] string callerFilePath = "",
                                   [CLN] int callerLineNumber = -1)
  {
    if (obj is not null)
    {
      return obj;
    }

    throw new StandardExpectationException("object is not null",
                                           $"{expression} is not null",
                                           callerMemberName,
                                           callerFilePath,
                                           callerLineNumber);
  }

  /// <summary> Throws when condition is <b> false </b>. </summary>
  /// <exception cref="StandardExpectationException"> <paramref name="condition" /> is <b> false </b> </exception>
  public static void Expect(string expectation,
                            bool condition,
                            [CAE(nameof(condition))] string conditionExpression = "",
                            [CMN] string callerMemberName = "",
                            [CFP] string callerFilePath = "",
                            [CLN] int callerLineNumber = -1)
  {
    if (condition)
    {
      return;
    }

    throw new StandardExpectationException(expectation,
                                           conditionExpression,
                                           callerMemberName,
                                           callerFilePath,
                                           callerLineNumber);
  }

  /// <summary> Throws when condition is <b> false </b>; exception captures additional data. </summary>
  /// <exception cref="StandardExpectationException"> <paramref name="condition" /> is <b> false </b> </exception>
  public static void Expect<T>(string expectation,
                               bool condition,
                               T data,
                               [CAE(nameof(condition))] string expectedConditionExpression = "",
                               [CAE(nameof(data))] string dataExpression = "",
                               [CMN] string callerMemberName = "",
                               [CFP] string callerFilePath = "",
                               [CLN] int callerLineNumber = -1)
  {
    if (condition)
    {
      return;
    }

    throw new StandardExpectationException(expectation,
                                           expectedConditionExpression,
                                           callerMemberName,
                                           callerFilePath,
                                           callerLineNumber)
      .WithNamedData(dataExpression, data);
  }
}
