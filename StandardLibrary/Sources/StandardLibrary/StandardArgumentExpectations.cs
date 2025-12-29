// StandardArgumentExpectations.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using System.Diagnostics.CodeAnalysis;
using angelof.dev.Exceptions;

namespace angelof.dev;

/// <summary> Provides methods for validating run-time expectations about method arguments. </summary>
/// <remarks>
///   All methods throw <see cref="ArgumentOutOfRangeException" /> derived exception when their expectation check
///   fails.
/// </remarks>
public static class StandardArgumentExpectations
{
  /// <summary> Throws when argument is <b> null </b>. </summary>
  /// <exception cref="StandardArgumentException"> <paramref name="argument" /> is <b> null </b> </exception>
  [return: NotNullIfNotNull(nameof(argument))]
  public static T ExpectArgumentNotNull<T>(T? argument,
                                           [CAE(nameof(argument))] string argumentName = "",
                                           [CMN] string callerMemberName = "",
                                           [CFP] string callerFilePath = "",
                                           [CLN] int callerLineNumber = -1)
  {
    return argument ??
           throw new StandardArgumentException("null",
                                               argumentName,
                                               "not null",
                                               callerMemberName,
                                               callerFilePath,
                                               callerLineNumber);
  }
}
