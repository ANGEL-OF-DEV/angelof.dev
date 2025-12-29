// ExceptionExtensions.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

namespace angelof.dev.Exceptions;

/// <summary> Provides extension for instances of <see cref="Exception" />. </summary>
public static class ExceptionExtensions
{
  extension<TException>(TException exception) where TException : Exception
  {
    /// <summary> Adds a named data to exception object. </summary>
    /// <returns>The original exception object</returns>
    public TException WithNamedData<T>(string name, T? data)
    {
      exception.Data.Add(name, data);
      return exception;
    }

    /// <summary> Adds to exception object, using the expression used for it as name. </summary>
    /// <returns>The original exception object</returns>
    public TException WithData<T>(T? data, [CAE(nameof(data))] string dataExpression = "")
    {
      exception.Data.Add(dataExpression, data);
      return exception;
    }
  }
}
