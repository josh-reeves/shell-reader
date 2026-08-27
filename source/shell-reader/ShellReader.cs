using System.Text;
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
    #region Constructor(s)
    public Terminal()
    {        
        Cursor = new TextCursor(this);

    }

    #endregion

    public ITextCursor Cursor { get; }

    public ConsoleKeyInfo ReadKey(bool intercept = false) => ((TextCursor)Cursor).ReadKey(intercept);

    public void Write(object? value = null) => ((TextCursor)Cursor).Write(value);

    public void WriteLine(object? value = null) => ((TextCursor)Cursor).WriteLine(value);

    private class TextCursor : ITextCursor
    {
        #region Fields
        private const char Escape = '\u001B';
        
        private int col;

        private string escapePrefix => $"{Escape}[";

        private Terminal terminal;

        #endregion

        public TextCursor(IConsole parent)
        {
            col = 0;

            terminal = (Terminal)parent;

        }

        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept);

            if (!char.IsControl(keyInfo.KeyChar))
            {
                col++;

            }
            
            return keyInfo;

        }

        public void Write(object? value = null)
        {
            Console.Write(value);

            if (value?.ToString()?.Contains(escapePrefix) ?? true)
            {
                return;

            }

            col += value?.ToString()?.Length ?? 0;
        
        }

        public void WriteLine(object? value = null)
        {
            Console.WriteLine(value);

            col = 0;
            
        }

        public void MoveUp(int count = 1) => Console.Write($"{escapePrefix}{count}A");
        
        public void MoveDown(int count = 1) => Console.Write($"{escapePrefix}{count}B");

        public void MoveLeft(int count = 1) => Console.Write($"{escapePrefix}{count}D");
        
        public void MoveRight(int count = 1) => Console.Write($"{escapePrefix}{count}C");
        
        public void SetColumn(int count) => Console.Write($"{escapePrefix}{count}G");
        
        public void ClearRemaining() => Console.Write($"{escapePrefix}K");

        /* This method is still very much a WIP. I think it should work now, but
         *  it's awkward. */
        public (int row, int col) GetPosition()
        {
            while (Console.KeyAvailable) { Console.ReadKey(true); }
            
            string dsr = string.Empty;

            DateTime timeout = DateTime.Now.AddMilliseconds(100);

            /* The cursor's column is still being tracked virtually with a
             *  private variable. This is gross, but it makes it safe to
             *  manually move the cursor back to the start of the line:*/
            SetColumn(1);

            /* The DSR ANSI sequence seems to work reliably as long as it's
                called before any other text: */
            Console.Write($"{escapePrefix}6n");
            Console.Out.Flush();

            ConsoleKeyInfo? keyInfo = null;

            while (keyInfo?.KeyChar != 'R' && DateTime.Now < timeout)
            {
                if (Console.KeyAvailable)
                {
                    keyInfo = Console.ReadKey(true);

                    dsr += keyInfo?.KeyChar;

                }                
                
            }

            dsr = dsr.Substring(2, dsr.Length - 3);

            string[] result = dsr.Split(';');

            /* Now that we have a DSR result with the cursor's row, move the
             *  cursor back to the correct/expected column:*/
            SetColumn(col + 1);

            /* If everything went well, return the row parsed from the DSR along
             *  with the tracked column. Increment the column by 1 because ANSI
             *  coordinates are 1-indexed:*/
            if (result.Count() >= 2 && int.TryParse(result[0], out int row))
            { 
                return (row, col + 1);

            }

            /* Console.CursorTop() can freeze in some situations. Use as last
                resort: */
            return (Console.CursorTop + 1, col + 1);

        }

        public void SetPosition(int row, int col) 
            => terminal.WriteLine($"{escapePrefix}{row};{col}H");

    }

}
