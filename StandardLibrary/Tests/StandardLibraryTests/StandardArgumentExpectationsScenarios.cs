// StandardArgumentExpectationsScenarios.cs | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]

using angelof.dev.Exceptions;

namespace tests.angelof.dev;

public static class StandardArgumentExpectationsScenarios
{
  [TestClass]
  public sealed class ExpectArgumentNotNullScenarios
  {
    [TestMethod]
    [DataRow(null)]
    public void ExpectArgumentNotNull_Throws_When_Argument_Is_Null(object? argumentOne)
    {
      var e = Assert.Throws<StandardArgumentException>(() => ExpectArgumentNotNull(argumentOne));
      Assert.AreEqual("Argument value is not expected and invalid. (Parameter 'argumentOne')",
                      e.StableMessage.ToString());
      Assert.AreEqual("null", e.ActualValue);
      Assert.AreEqual("not null", e.ExpectedValue);
    }

    [TestMethod]
    [DataRow(13)]
    public void ExpectArgumentNotNull_Returns_Argument_Value_When_Not_Null(object? argumentOne)
    {
      var actual = ExpectArgumentNotNull(argumentOne);
      Assert.AreEqual(argumentOne, actual);
    }
  }
}
