using Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader, IDebuggable
{
    #region Fields
    private string input;

    private ITextCursor cursor => Terminal.Cursor;

    #endregion

    public ShellReader(string prompt = "", IConsole? terminal = null, IDictionary<ConsoleKeyInfo, Func<string, string>>? keyMap = null)
    {
        input = string.Empty;

        Prompt = prompt;
        
        KeyMap = keyMap ?? new Dictionary<ConsoleKeyInfo, Func<string, string>>();

        Terminal = terminal ??= new Terminal();

    }

    #region Events
    public event EventHandler<ConsoleKeyInfo>? InputReceived;

    #endregion

    #region Properties
    public bool IsReading { get; set; }

    public string Prompt { get; set; }

    public IConsole Terminal { get; set; }

    public IDictionary<ConsoleKeyInfo, Func<string, string>> KeyMap { get; }

    public IDebugger? Debugger { get; set; }

    #endregion

    #region Methods
    private Func<string, string>? RetrieveKeyMap(IDictionary<ConsoleKeyInfo, Func<string, string>> map, ConsoleKeyInfo keyInfo)
    {
        foreach (ConsoleKeyInfo compare in map.Keys)
        {
            Debugger?.WriteLine($"Comparing {keyInfo.Modifiers}{keyInfo.Key} to {compare.Modifiers}{compare.Key}", ["INPUT"]);

            if (MeetsKeyModifierMinimum(keyInfo, compare))
            {
                return map[compare];
                
            }
            
        }
        
        return null;

    }

    private bool MeetsKeyModifierMinimum(ConsoleKeyInfo keyInfo, ConsoleKeyInfo compare)
    {
        if (keyInfo.Key == compare.Key && (keyInfo.Modifiers & compare.Modifiers) == compare.Modifiers)
        {
            Debugger?.WriteLine("Key and minimum modifier requirements met.", ["INPUT"]);
            return true;
        }
        
        return false;
        
    }

    private void BroadcastInput(ConsoleKeyInfo keyInfo)
    {
        Debugger?.WriteLine($"Keypress received: {keyInfo.Modifiers}{keyInfo.Key}", ["INPUT"]);

        InputReceived?.Invoke(this, keyInfo);
        
    }

    public string Read(string? prompt = null)
    {
        input = string.Empty;

        prompt ??= Prompt;
        
        Terminal.Write(prompt);

        IsReading = true;

        while (IsReading)
        {
            ConsoleKeyInfo keyInfo = Terminal.ReadKey(intercept: true);

            BroadcastInput(keyInfo);

            Func<string, string>? func = RetrieveKeyMap(KeyMap, keyInfo);

            if (func is not null)
            {
                Debugger?.WriteLine($"Executing mapped action: {func.Method.Name}", ["INPUT"]);

                input = func(input);

                continue;

            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                Debugger?.WriteLine($"Adding character to input string: {keyInfo.KeyChar}", ["INPUT"]);

                input += keyInfo.KeyChar;
                
                Terminal.Write(keyInfo.KeyChar);

            }

        }

        Debugger?.WriteLine($"Exiting input loop.", ["INPUT"]);

        Terminal.WriteLine();

        return input;

    }

    public void ClearLine(int startPos = 0)
    {
        cursor.SetColumn(1 + startPos);

        cursor.ClearRemaining();

    }

    public void Insert(string insert, int startPos = 0)
    {
        cursor.SetColumn(1 + startPos);

        Terminal.Write(insert);

    }

    #endregion

    #region Structs


    #endregion

}

public class Terminal : IConsole
{
    public Terminal()
    {
        Cursor = new TextCursor();
    
    }

    public ITextCursor Cursor { get; }

    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

    public void Write(object? value = null) => Console.Write(value);

    public void WriteLine(object? value = null) => Console.WriteLine(value);

    private struct TextCursor : ITextCursor
    {
        private const char Escape = '\u001B';

        private string escapePrefix => $"{Escape}[";

        private IConsole terminal;

        public TextCursor(IConsole parent)
        {
            terminal = parent;

        }

        public void MoveUp(int count = 1) => terminal.Write($"{escapePrefix}{count}A");
        
        public void MoveDown(int count = 1) => terminal.Write($"{escapePrefix}{count}B");

        public void MoveLeft(int count = 1) => terminal.Write($"{escapePrefix}{count}D");
        
        public void MoveRight(int count = 1) => terminal.Write($"{escapePrefix}{count}C");
        
        public void SetColumn(int count) => terminal.Write($"{escapePrefix}{count}G");
        
        public void ClearRemaining() => terminal.Write($"{escapePrefix}K");

        /* This method is very much a WIP. Right now it causes the shell to hang
        *  unless it's launched before the prompt is written. 
        *  Not sure why yet.*/
        public (int row, int col) GetPosition()
        {
            int row = Console.CursorTop,
                col = Console.CursorLeft;
            
            return (row, col);
        
        }

        public void SetPosition(int row, int col) 
            => terminal.WriteLine($"{escapePrefix}{row};{col}H");

    }

}
