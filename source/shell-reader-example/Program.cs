namespace ShellReaderExample;

using ShellReader;

class Program
{
    static void Main(string[] args)
    {
        string input = string.Empty,
               exitStr = "exit",
               passwordStr = "pass",
               programDir = AppContext.BaseDirectory,
               mask = string.Empty,
               passMsg = $"Enter a sequence of any length to use as a mask. Current mask: {mask}",
               welcomeMsg = "ShellReader Example. Type 'pass' to enter password mode. Type 'exit' to quit:";

        ShellReader reader = new(
            prompt: "$ ",
            mask: mask);
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

        reader.Terminal.WriteLine(welcomeMsg);

        while(!string.Equals(input, exitStr))
        {
            input = reader.Read();

            if (string.Equals(input, passwordStr))
            {
                reader.Terminal.WriteLine(passMsg);
                mask = reader.Read(prompt: "");

                input = reader.ReadPassword(mask: mask);
    
            }

            reader.Terminal.WriteLine($"Input Received: {input}");

        }

    }

}
