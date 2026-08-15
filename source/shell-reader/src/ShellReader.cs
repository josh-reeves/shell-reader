using Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader, IDebuggable
{
    private string input;

    public ShellReader(string prompt = "", IDictionary<ConsoleKeyInfo, Func<string, string>>? keyMap = null)
    {
        input = string.Empty;

        Prompt = prompt;
        
        KeyMap = keyMap ?? new Dictionary<ConsoleKeyInfo, Func<string, string>>();

        Cursor = new TextCursor();
        
    }

    #region Events
    public event EventHandler<ConsoleKeyInfo>? InputReceived;

    #endregion

    #region Properties
    public bool IsReading { get; set; }

    public string Prompt { get; set; }

    public ITextCursor Cursor { get; }

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
        
        Console.Write(prompt);

        IsReading = true;

        while (IsReading)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

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
                
                Console.Write(keyInfo.KeyChar);

            }

        }

        Debugger?.WriteLine($"Exiting input loop.", ["INPUT"]);

        Console.WriteLine();

        return input;

    }

    public void ClearLine(int startPos = 0)
    {
        Cursor.SetColumn(1 + startPos);

        Cursor.ClearRemaining();

    }

    public void Insert(string insert, int startPos = 0)
    {
        Cursor.SetColumn(1 + startPos);

        Console.Write(insert);

    }

    #endregion

    #region Structs
    private struct TextCursor : ITextCursor
    {
        private const char Escape = '\u001B';
        private string escapePrefix => $"{Escape}[";

        public TextCursor() {}

        public void MoveUp(int count = 1) => Console.Write($"{escapePrefix}{count}A");
        
        public void MoveDown(int count = 1) => Console.Write($"{escapePrefix}{count}B");

        public void MoveLeft(int count = 1) => Console.Write($"{escapePrefix}{count}D");
        
        public void MoveRight(int count = 1) => Console.Write($"{escapePrefix}{count}C");
        
        public void SetColumn(int count) => Console.Write($"{escapePrefix}{count}G");
        
        public void ClearRemaining() => Console.Write($"{escapePrefix}K");

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
            => Console.WriteLine($"{escapePrefix}{row};{col}H");

    }

    #endregion

}