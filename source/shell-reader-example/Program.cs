namespace ShellReaderExample;

using ShellReader;

class Program
{
    static void Main(string[] args)
    {
        ShellReader reader = new("$ ");
        ShellControls controls = new(reader);

        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false), controls.Enter);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false), controls.Backspace);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false), controls.UpArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false), controls.DownArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.A, false, false, true), controls.CtrlA);

        string input = string.Empty,
               exitStr = "exit",
               welcomeMsg = "ShellReader Example. Type exit to quit:";

        Console.WriteLine(welcomeMsg);

        while(input.ToLower() != exitStr)
        {
            input = reader.Read();

        }

    }
}
