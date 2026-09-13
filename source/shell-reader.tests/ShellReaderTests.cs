using ShellReader.Tests.Data;

namespace ShellReader.Tests;

public class ShellReaderTests
{
    #region Constructor(s)
    public static TheoryData<ConsoleKeyInfo[], string> TestData = new() {};

    #endregion

    #region Methods
    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_NoPassword_DisplaysAndReturnsInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        Queue<ConsoleKeyInfo> input = new(keyStrokes);

        VirtualConsole console = new()
        {
            InputStream = input

        };

        ShellReader reader = new(terminal: console);
        ShellControls controls = new(reader);

        reader.KeyMap.Add(new('\0', ConsoleKey.Enter, false, false, false), controls.Enter);

        string compare = reader.Read();

        Assert.Equal(test, compare);

    }

    #endregion

    #region Classes & Structs
    private class VirtualConsole : Terminal
    {
        public VirtualConsole()
        {
            InputStream = new();

        }

        public override ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (InputStream.Count <= 0)
            {
                return new();

            }

            return InputStream.Dequeue();

        }

        public Queue<ConsoleKeyInfo> InputStream { get; set; }
        
    }

    #endregion
}
