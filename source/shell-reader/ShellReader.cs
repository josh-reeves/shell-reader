using ShellReader.Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader
{
    #region Fields
    private ITextCursor cursor => Terminal.Cursor;

    #endregion

    public ShellReader(string prompt = "", IConsole? terminal = null, IDictionary<ConsoleKeyInfo, Func<string, string>>? keyMap = null)
    {
        Prompt = prompt;
        
        KeyMap = keyMap ?? new Dictionary<ConsoleKeyInfo, Func<string, string>>();

        terminal ??= new Terminal();
        
        Terminal = terminal;

    }

    #region Events
    public event EventHandler<ConsoleKeyInfo>? InputReceived;

    #endregion

    #region Properties
    public bool IsReading { get; set; }

    public string Prompt { get; set; }

    public IConsole Terminal { get; set; }

    public IDictionary<ConsoleKeyInfo, Func<string, string>> KeyMap { get; }

    #endregion

    #region Methods
    private Func<string, string>? RetrieveKeyMap(IDictionary<ConsoleKeyInfo, Func<string, string>> map, ConsoleKeyInfo keyPress)
    {
        ConsoleKeyInfo temp = new(keyPress.KeyChar, keyPress.Key, false, false, false);

        Func<string, string>? result = null;

        foreach (ConsoleKeyInfo compare in map.Keys)
        {
            if (MeetsKeyModifierMinimum(keyPress, compare) && MeetsKeyModifierMinimum(compare, temp))
            {
                result = map[compare];
                temp = compare;

            }
            
        }

        return result;

    }

    private bool MeetsKeyModifierMinimum(ConsoleKeyInfo keyPress, ConsoleKeyInfo compare)
    {
        if (keyPress.Key == compare.Key && (keyPress.Modifiers & compare.Modifiers) == compare.Modifiers)
        {
            return true;

        }
        
        return false;
        
    }

    private void BroadcastInput(ConsoleKeyInfo keyInfo)
    {
        InputReceived?.Invoke(this, keyInfo);
        
    }

    public string ReadPassword(string? prompt = null)
    {
        return Read(prompt);

    }

    public string Read(string? prompt = null)
    {
        string input = string.Empty;

        prompt ??= Prompt;

        Terminal.Write(prompt);

        IsReading = true;

        while (IsReading)
        {
            ConsoleKeyInfo keyPress = Terminal.ReadKey(intercept: true);

            BroadcastInput(keyPress);

            Func<string, string>? func = RetrieveKeyMap(KeyMap, keyPress);

            if (func is not null)
            {
                input = func(input);

                continue;

            }

            if (!char.IsControl(keyPress.KeyChar))
            {
                input += keyPress.KeyChar;
                
                Terminal.Write(keyPress.KeyChar);

            }

        }

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

}

public class Terminal : IConsole
{
    public Terminal()
    {
        Cursor = new TextCursor(this);
    
    }

    public ITextCursor Cursor { get; }

    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

    public void Write(object? value = null) => Console.Write(value);

    public void WriteLine(object? value = null) => Console.WriteLine(value);

    private class TextCursor : ITextCursor
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
            


            DateTime timeout = DateTime.Now.AddSeconds(2);

            string input = string.Empty;

            Console.Out.Flush();
            Console.Write("\x1b[6n");
            Console.Out.Flush();
            Console.WriteLine(Console.OpenStandardInput().ReadByte());

            Console.OpenStandardInput().Flush();

            return (row, col);
        
        }

        public void SetPosition(int row, int col) 
            => terminal.WriteLine($"{escapePrefix}{row};{col}H");

    }

}
