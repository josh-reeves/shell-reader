using Xunit.Abstractions;
using Xunit.Extensions;

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
        ShellReader reader = new();

        Assert.Equal((1,1), reader.Terminal.Cursor.GetPosition());

    }

    #endregion

}