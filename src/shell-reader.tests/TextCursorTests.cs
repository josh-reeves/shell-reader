using Xunit.Abstractions;

namespace ShellReader.Tests;

public class TextCursorTests
{
    #region Fields
    ShellReader reader;

    #endregion

    #region Constructor(s)
    public TextCursorTests()
    {
        reader = new();

    }

    #endregion

    #region Methods
    [Theory]
    [InlineData("", 1, 1)]
    [InlineData("test", 1, 5)]
    [InlineData("with\nnewlines", 2, 9)]
    [InlineData("with\nmore\nnewlines\n", 4, 1)]
    public void GetCursorPosition_AnyLine_ReturnsCoordinates(string input, int testRow, int testCol)
    {
        reader.Terminal.Write(input);

        Assert.Equal((testRow, testCol), reader.Terminal.Cursor.GetPosition());

    }

    #endregion

}