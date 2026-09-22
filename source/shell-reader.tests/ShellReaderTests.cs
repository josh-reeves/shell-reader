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
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false), controls.Enter);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false), controls.Backspace);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false), controls.UpArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false), controls.DownArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false), controls.LeftArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false), controls.RightArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false), controls.Home);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.A, false, false, true), controls.CtrlA);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false), controls.End);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.C, false, false, true), controls.CtrlC);

    }

    #endregion

    #region Methods
    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_NoPasswordNoPrompt_DisplayAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
       ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);

        string compare = reader.Read() ?? string.Empty;

        Assert.Equal(test, compare);

    }

    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_NoPasswordAnyPrompt_DisplayAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] prompts = ["$ ", "# ", ": ", "% ", "Enter Command: "];

        foreach(string prompt in prompts)
        {
            ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
            
            string compare = reader.Read(prompt: prompt) ?? string.Empty;

            if (compare != test)
            {
                Assert.Fail();

            }

        }

    }

    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void Read_AsPasswordNoPrompt_HideAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);

        string compare = reader.Read(isPassword: true) ?? string.Empty;

        Assert.Equal(test, compare);

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
    public void ReadPassword_AnyMaskNoPrompt_PromptHideAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] masks = ["*", "pass", "123", "-_-10<>/\\?.';,[]{}|`~:", ""];  

        foreach(string mask in masks)
        {
            ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
            
            string compare = reader.ReadPassword(mask: mask) ?? string.Empty;

            if (compare != test)
            {
                Assert.Fail();

            }

        }

    }

    [Theory]
    [ClassData(typeof(ShellReaderTestData))]
    public void ReadPassword_AnyMaskAnyPrompt_PromptHideAndReturnInput(ConsoleKeyInfo[] keyStrokes, string test)
    {
        string[] prompts = ["$ ", "# ", ": ", "% ", "Enter Command: "];
        string[] masks = ["*", "pass", "123", "-_-10<>/\\?.';,[]{}|`~:", ""];  

        foreach(string prompt in prompts)
        {
            foreach(string mask in masks)
            {
                ((VirtualConsole)reader.Terminal).InputStream = new(keyStrokes);
                
                string compare = reader.ReadPassword(prompt: prompt, mask: mask) ?? string.Empty;

                if (compare != test)
                {
                    Assert.Fail();

                }

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
