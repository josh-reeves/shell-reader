using ShellReader.Tests.Data;

namespace ShellReader.Tests;

public class ShellReaderTests
{
    ShellReader reader;

    #region Constructor(s)
    public ShellReaderTests()
    {
        reader = new(terminal: new VirtualConsole());

        ShellControls controls = new(reader);
        reader.KeyMap.Add(new('\0', ConsoleKey.Enter, false, false, false), controls.Enter);

    }

    #endregion

    #region Methods
    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_NoPassword_DisplaysAndReturnsInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
       ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);

        string compare = reader.Read();

        Assert.Equal(test, compare);

    }

    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_Password_HidesAndReturnsInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] masks = ["*", "pass", "123", "-_-10<>/\\?.';,[]{}|`~:"];  

        foreach(string mask in masks)
        {
            ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
            
            string compare = reader.ReadPassword(mask: mask);

            if (compare != test)
            {
                Assert.Fail();

            }

        }

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
