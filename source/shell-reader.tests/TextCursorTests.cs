using Xunit.Abstractions;

namespace ShellReader.Tests;

public class TextCursorTests
{
    #region Fields
    ITestOutputHelper outputHelper;

    #endregion

    public TextCursorTests(ITestOutputHelper testOutputHelper)
    {
        outputHelper = testOutputHelper;

    }

    #region Methods
    [Fact]
    public void GetCursorPosition_NewLine_ReturnsCoordinates()
    {

    }

    #endregion

}