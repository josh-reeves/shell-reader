namespace ShellReader.Tests;

public class ShellReaderTests
{
    [Fact]
    public async Task Read_NoPassword_DisplaysAndReturnsInput()
    {
        Queue<ConsoleKeyInfo> input = new(
            [
                new('t', ConsoleKey.T, false, false, false),
                new('e', ConsoleKey.E, false, false, false),
                new('s', ConsoleKey.S, false, false, false),
                new('t', ConsoleKey.T, false, false, false),
                new('\n', ConsoleKey.Enter, false, false, false)
      
            ]

        );

        VirtualConsole console = new()
        {
            InputStream = input

        };

        ShellReader reader = new(terminal: console);
        ShellControls controls = new(reader);

        reader.KeyMap.Add(new('\0', ConsoleKey.Enter, false, false, false), controls.Enter);

        string output = reader.Read();

        Assert.Equal("test", output);

    }

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

}
