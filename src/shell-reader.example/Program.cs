namespace ShellReader.Example;

class Program
{
    static void Main(string[] args)
    {
        string input = string.Empty,
               exitStr = "exit",
               passwordStr = "pass",
               programDir = AppContext.BaseDirectory,
               prompt = "$ ",
               mask = string.Empty,
               passMsg = $"Enter a sequence of any length to use as a mask. Current mask: {mask}",
               welcomeMsg = "ShellReader Example. Type 'pass' to enter password mode. Type 'exit' to quit:";

        ShellReader reader = new(prompt: prompt, mask: mask);
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
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.D, false, false, true), controls.CtrlD);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false), controls.Del);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.F, false, false, true), controls.CtrlF);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.H, false, false, true), controls.CtrlH);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.J, false, false, true), controls.CtrlJ);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.L, false, false, true), controls.CtrlL);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.N, false, false, true), controls.CtrlN);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.P, false, false, true), controls.CtrlP);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.B, false, false, true), controls.CtrlB);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Insert, false, false, false), controls.Insert);

        Console.TreatControlCAsInput = true;

        while(!string.Equals(input, exitStr))
        {
            reader.Terminal.WriteLine(welcomeMsg);
            input = reader.Read() ?? string.Empty;

            if (string.Equals(input, passwordStr))
            {
                reader.Terminal.WriteLine(passMsg);
                mask = reader.Read(prompt: "") ?? string.Empty;

                input = reader.ReadPassword(mask: mask) ?? string.Empty;
    
            }

            reader.Terminal.WriteLine($"Input Received: {input}");

        }

    }

}
