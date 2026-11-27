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
        controls.RegisterAllWithDefaults();

    }

    #endregion

    #region Methods
    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_NoPasswordAnyPrompt_DisplayAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] prompts = [string.Empty, "$ ", "# ", ": ", "% ", "Enter Command: "];

        if (reader.Terminal is not VirtualConsole virtualConsole)
        {
            Assert.Fail();

            return;

        }

        foreach(string prompt in prompts)
        {
            virtualConsole.InputStream = new(keyStrokes);
            
            string compare = reader.Read(prompt: prompt) ?? string.Empty;

            if (compare != test || virtualConsole.TestBuffer != prompt + test)
            {
                Assert.Fail();

            }

        }

    }

    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_AsPasswordAnyPrompt_HideAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] prompts = ["$ ", "# ", ": ", "% ", "Enter Command: "];

        foreach(string prompt in prompts)
        {
            ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
            
            string compare = reader.Read(prompt: prompt, isPassword: true) ?? string.Empty;

            if (compare != test)
            {
                Assert.Fail();

            }

        }

    }


    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void ReadPassword_AnyPrompt_HideAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] prompts = ["$ ", "# ", ": ", "% ", "Enter Command: "];

        foreach(string prompt in prompts)
        {
            ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
            
            string compare = reader.Read(prompt: prompt, isPassword: true) ?? string.Empty;

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
            Cursor.IsVirtual = true;

            TestBuffer = string.Empty;
            InputStream = new();

        }

        #region Properties
        public string TestBuffer { get; private set; }

        public Queue<ConsoleKeyInfo> InputStream { get; set; }
        
        #endregion

        #region Methods
        public override ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (InputStream.Count <= 0)
            {
                return new();

            }

            return InputStream.Dequeue();

        }

        public override void WriteLine(object? value = null)
        {
            TestBuffer = Buffer.ToString() ?? string.Empty;

            base.WriteLine(value);
        
        }

        #endregion

    }

    #endregion
}
