using System.Text;
using ShellReader.Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader
{
    #region Fields
    private string mask;

    private ITextCursor cursor => Terminal.Cursor;

    #endregion

    public ShellReader(string prompt = "", IConsole? terminal = null, IDictionary<ConsoleKeyInfo, Func<string, string>>? keyMap = null)
    {
        mask = string.Empty;

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

    public bool IsPassword { get; private set; }

    public string Prompt { get; private set; }

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

    private string UpdateTextAtCursor(string original, string insert)
    {
        string updated = original;

        int textLength = IsPassword ? original.Length * mask.Length : original.Length;

        if (Terminal.Cursor.Column - Prompt.Length <= textLength)
        {
            int i = Terminal.Cursor.Column - Prompt.Length - 1;

            updated = original.Remove(i, insert.Length).Insert(i, insert);
        
        }
        else
        {
            updated += insert;
            
        }

        insert = IsPassword ? mask : insert;

        Terminal.Write(insert);
    
        return updated;

    }

    public string ReadPassword(string? prompt = null, string maskSeq = "")
    {
        mask = maskSeq;

        return Read(prompt, true);

    }

    public string Read(string? prompt = null, bool isPassword = false)
    {
        string input = string.Empty,
               temp = Prompt;

        IsPassword = isPassword;

        if (prompt is not null)
        {
            Prompt = prompt;

        }

        Terminal.Write(Prompt);

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
                input = UpdateTextAtCursor(input, keyPress.KeyChar.ToString());

            }

        }

        Terminal.WriteLine();

        Prompt = temp;

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
    #region Fields
    private readonly TextCursor cursor;

    #endregion

    #region Constructor(s)
    public Terminal()
    {        
        cursor = new TextCursor();

    }

    #endregion

    #region Properties
    public ITextCursor Cursor { get => cursor; }

    #endregion

    #region Methods
    public ConsoleKeyInfo ReadKey(bool intercept = false) => cursor.ReadKey(intercept);

    public void Write(object? value = null) => cursor.Write(value);

    public void WriteLine(object? value = null) => cursor.WriteLine(value);

    #endregion

    #region Subclasses
    private class TextCursor : ITextCursor
    {
        #region Fields
        private const char Escape = '\u001B';
        
        private int col;
        private string escapePrefix => $"{Escape}[";

        #endregion

        #region Constructor(s)
        public TextCursor()
        {
            col = 0;

        }

        #endregion

        #region Properties
        public int Column { get => col + 1; }

        public int Row { get => GetPosition().row; }

        #endregion

        #region Methods
        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept);

            if (!intercept && !char.IsControl(keyInfo.KeyChar))
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

        public void MoveUp(int count = 1) => Write($"{escapePrefix}{count}A");
        
        public void MoveDown(int count = 1) => Write($"{escapePrefix}{count}B");

        public void MoveLeft(int count = 1) => Write($"{escapePrefix}{count}D");
        
        public void MoveRight(int count = 1) => Write($"{escapePrefix}{count}C");
        
        public void SetColumn(int count)
        {
            Write($"{escapePrefix}{count}G");

            col = count - 1;

        }
        
        public void ClearRemaining() => Write($"{escapePrefix}K");

        /* This method is still very much a WIP. I think it should work now, but
         *  it's awkward. A better alternative would probably be importing from 
         *  libc and properly disabling canonical mode on linux systems, but 
         *  that's gross in its own way. */
        public (int row, int col) GetPosition()
        {
            while (Console.KeyAvailable) { Console.ReadKey(true); }
            
            string dsr = string.Empty;

            DateTime timeout = DateTime.Now.AddMilliseconds(100);

            /* The cursor's column is still being tracked virtually with a
             *  private variable. This is gross, but it makes it safe to
             *  manually move the cursor back to the start of the line:*/
            int temp = col;
            SetColumn(1);

            /* The DSR ANSI sequence seems to work reliably as long as it's
                called before any other text: */
            Write($"{escapePrefix}6n");
            Console.Out.Flush();

            /* Now that we have a DSR result with the cursor's row, move the
             *  cursor back to the correct/expected column:*/
            col = temp;
            SetColumn(col + 1);

            // Now we can capture the DSR:
            ConsoleKeyInfo? keyInfo = null;

            while (keyInfo?.KeyChar != 'R' && DateTime.Now < timeout)
            {
                if (Console.KeyAvailable)
                {
                    keyInfo = ReadKey(true);

                    dsr += keyInfo?.KeyChar;

                }                
                
            }

            dsr = dsr.Substring(2, dsr.Length - 3);

            string[] result = dsr.Split(';');

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
            => Write($"{escapePrefix}{row};{col}H");

        #endregion

    }

    #endregion

}
