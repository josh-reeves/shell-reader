using System.Collections;
using System.Runtime.InteropServices;
using ShellReader.Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader
{
    #region Fields
    private ITextCursor cursor => Terminal.Cursor;

    #endregion

    public ShellReader(bool insertMode = true, string prompt = "", string mask = "", string[]? separators = null, IConsole? terminal = null, IDictionary<ConsoleKeyInfo, Func<string, string?>>? keyMap = null)
    {
        InsertMode = insertMode;
        Prompt = prompt;
        Mask = mask;
        Separators = separators ?? [];
        KeyMap = keyMap ?? new Dictionary<ConsoleKeyInfo, Func<string, string?>>();

        Terminal = terminal ??= new Terminal();

        CaptureText = UpdateTextAtCursor;

    }

    #region Events
    public event EventHandler<ConsoleKeyInfo>? InputReceived;

    #endregion

    #region Properties
    public bool IsReading { get; set; }

    public bool IsPassword { get; private set; }

    public bool InsertMode { get; set; }

    public string Prompt { get; private set; }

    public string Mask { get; private set; }

    public IConsole Terminal { get; set; }

    public Func<string, string, string> CaptureText { get; set; }

    public string[] Separators { get; set; }

    public IDictionary<ConsoleKeyInfo, Func<string, string?>> KeyMap { get; set; }

    #endregion

    #region Methods
    private string UpdateTextAtCursor(string original, string insert)
    {
        int charIndex = cursor.Index - Prompt.Length;
        string updated = original,
               displayText = insert;        

        if (IsPassword)
        {
            displayText = Mask;

            bool adjustIndex = charIndex > original.Length || 
                               string.IsNullOrEmpty(Mask);

            charIndex = adjustIndex ? original.Length : charIndex;

        }

        // This still needs work:
        if (!InsertMode && cursor.Index - Prompt.Length < updated.Length)
        {
            int remaining = updated[charIndex..].Length,
                maxLength = insert.Length < remaining ? insert.Length : remaining;

            cursor.DeleteCharacter();
            updated = updated.Remove(charIndex, maxLength);
            
        }

        updated = updated.Insert(charIndex, insert);

        Terminal.Write(displayText);

        return updated;
    
    }

    private Func<string, string?>? RetrieveKeyMap(IDictionary<ConsoleKeyInfo, Func<string, string?>> map, ConsoleKeyInfo keyPress)
    {
        ConsoleKeyInfo temp = new(keyPress.KeyChar, keyPress.Key, false, false, false);

        Func<string, string?>? result = null;

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

    public string? ReadPassword(string? prompt = null, string mask = "")
    {
        string temp = Mask;

        Mask = mask;

        string? input = Read(prompt, true);

        Mask = temp;
            
        return input;

    }

    public string? Read(string? prompt = null, bool isPassword = false)
    {
        string? input = string.Empty;
        string       temp = Prompt;

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

            Func<string, string?>? func = RetrieveKeyMap(KeyMap, keyPress);

            if (func is not null)
            {
                input = func(input ?? string.Empty);

                continue;

            }

            if (!char.IsControl(keyPress.KeyChar) && input is not null)
            {
                input = CaptureText(input, keyPress.KeyChar.ToString());

            }

        }

        Terminal.WriteLine();

        Prompt = temp;

        return input;

    }

    #endregion

}

public class Terminal : IConsole
{
    #region Fields
    private const int STDINFILE = 0;
    private const int TCSANOW = 0;
    private const uint ICANON = 0x00000002;
    private const uint ECHO = 0x00000008;

    private readonly TextCursor cursor;

    private string buffer;

    #endregion

    #region Constructor(s)
    public Terminal()
    {
        cursor = new TextCursor(this);
        buffer = string.Empty;

    }

    #endregion

    #region Properties
    public IEnumerable Buffer
    {
        get => buffer;

        set
        {
            if (value is not string str)
            {
                return;

            }

            int lengthDiff = buffer.Length - str.Length;
            
            buffer = value as string ?? buffer;

            (int row, int col) curPos = (Cursor.Row, Cursor.Column);

            Cursor.SetPosition(1, 1);

            Console.WriteLine(buffer + new string(' ', Math.Max(lengthDiff, 0) + 1));

            Cursor.SetPosition(curPos.row, curPos.col);

        }

    }

    public ITextCursor Cursor { get => cursor; }

    #endregion

    #region Methods
    [DllImport("libc", EntryPoint = "tcgetattr")]
    private static extern int TCGetAttr(int fd, out Termios termios);

    [DllImport("libc", EntryPoint = "tcsetattr")]
    private static extern int TCSetAttr(int fd, int optional, ref Termios termios); 

    [DllImport("libc", EntryPoint = "read")]
    private static extern IntPtr Read(int fd, out byte buf, UIntPtr count);

    public virtual ConsoleKeyInfo ReadKey(bool intercept = false) 
        => Console.ReadKey(intercept);

    public virtual void Write(object? value = null) 
        => cursor.Write(value?.ToString() ?? "");

    public virtual void WriteLine(object? value = null)
        => cursor.WriteLine(value?.ToString() ?? "");

    #endregion

    #region Structures
    /* "The termios functions describe a general terminal interface that
     *  is provided to control asynchronous communications ports."
     *  https://man7.org/linux/man-pages/man3/termios.3.html
     *
     *  This is a C thing, so the struct will need to be sequential: */
    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public uint c_iflag; // Input modes
        public uint c_oflag; // Output modes
        public uint c_cflag; // Control modes
        public uint c_lflag; // Local modes

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] c_cc; // Special characters
        
    }

    #endregion

    #region Subclasses
    private class TextCursor : ITextCursor
    {
        #region Fields
        private const char Escape = '\u001B';

        private int startingRow;
        private string escapePrefix => $"{Escape}[";

        private (int row, int col) virtualCursor;

        private Terminal terminal;

        #endregion

        #region Constructor(s)
        public TextCursor(IConsole parent)
        {
            startingRow = GetAbsolutePosition().row;
            terminal = (Terminal)parent;

            virtualCursor = (1, 1);

            IsVirtual = Console.IsInputRedirected;
            
        }

        #endregion

        #region Properties
        public bool IsVirtual { get; set; }

        public int Column 
        { 
            get => virtualCursor.col;
        
            private set
            {
                virtualCursor.col = value;

                if (virtualCursor.col > Console.WindowWidth)
                {
                    virtualCursor.row += 1;

                    virtualCursor.col -= Console.WindowWidth;

                }
                
            }

        }

        public int Row 
        { 
            get => virtualCursor.row; 

            private set => virtualCursor.row = value;

        }

        public int Index
        {
            get => ((Row - 1) * Console.WindowWidth) + Column - 1;
        
        }

        #endregion

        #region Methods
        /// <summary>
        /// Wraps the cursor position to the adjacent line if the cursor is 
        /// moved beyond the current line.
        /// </summary>
        /// <param name="count">
        /// The number of positions to move the cursor.
        /// </param>
        /// <returns>
        /// The number of positions to move the cursor remaining after wrapping.
        /// </returns>
        private int Wrap(int count)
        {
            int remaining = count;
            
            while (Column + count > Console.WindowWidth)
            {
                remaining -= Console.WindowWidth - Column + 1;

                MoveDown();
                SetColumn(1);

            }
            
            while (Column + count < 1)
            {
                remaining = Column + count;
                
                MoveUp();
                SetColumn(Console.WindowWidth);

            }

            return remaining;

        }

        private (int row, int col) GetAbsolutePositionLibC()
        {
            TCGetAttr(STDINFILE, out Termios original);

            try
            {
                Termios raw = original;
                
                /* Ensure cannoncial and echo flags are disabled by inverting them, 
                *  ANDing them against the original values, and assigning the 
                *  result:*/
                raw.c_lflag &= ~(ICANON | ECHO);

                TCSetAttr(STDINFILE, TCSANOW, ref raw);

                while (Console.KeyAvailable) { Console.ReadKey(true); }

                char terminator = 'R';       
                string dsr = string.Empty;

                DateTime timeout = DateTime.Now.AddMilliseconds(100);

                terminal.Write($"{escapePrefix}6n");

                while (!dsr.EndsWith(terminator) && DateTime.Now < timeout)
                {
                    long bytesRead = Read(STDINFILE, out byte b, 1);
                    
                    if (bytesRead <= 0)
                    {
                        continue;

                    }

                    dsr += (char)b;

                }

                dsr = dsr.Substring(2, dsr.Length - 3);

                string[] result = dsr.Split(';');

                int row = int.Parse(result[0]), 
                    col = int.Parse(result[1]);
                    
                return (row, col);

            }
            catch
            {                
                return (Row, Column);

            }
            finally
            {
                // Restore original terminal mode:
                TCSetAttr(STDINFILE, TCSANOW, ref original);
                
            }

        }

        private (int row, int col) GetAbsolutePosition()
        {
            if (IsVirtual)
            {
                return GetPosition();
                
            }

            if (OperatingSystem.IsWindows())
            {
                return (Console.CursorTop + 1, Console.CursorLeft + 1);
            
            }

            return GetAbsolutePositionLibC();

        }

        public (int row, int col) GetPosition()
        {
            return (Row, Column);

        }

        public void SetPosition(int row, int col) 
        {
            row = Math.Max(row, 1);
            col = Math.Clamp(col, 1, Console.WindowWidth);

            /* The cursor is only aware of input that is passed through its
             *  methods. This means that output from other programs cannot be
             *  reliably tracked. As a result, the cursor's line coordinates are
             *  based on the current line at the point the cursor is used.*/
            row += startingRow - 1;

            terminal.Write($"{escapePrefix}{row};{col}H");

            Row = row - (startingRow - 1);
            Column = col;

        }

        public void MoveUp(int count = 1) 
        {
            terminal.Write($"{escapePrefix}{count}A");
        
            Row -= count;

        }

        public void MoveDown(int count = 1) 
        {
            terminal.Write($"{escapePrefix}{count}B");

            Row += count;
            
        }

        public void MoveLeft(int count = 1)
        {
            if (count == 0)
            {
                return;

            }

            if (Column - count < 1)
            {
                count -= count - Column - 1;
            
            }

            terminal.Write($"{escapePrefix}{count}D");

            Column -= count;

        }
        
        public void MoveRight(int count = 1) 
        {
            if (count == 0)
            {
                return;

            }

            if (Column + count > Console.WindowWidth)
            {
                count -= Console.WindowWidth - Column;
                
            }

            terminal.Write($"{escapePrefix}{count}C");

            Column += count;

        }

        public void Advance(int count)
        {
            count = Wrap(count);

            if (count > 0)
            {
                MoveRight(count);

            }

            if (count < 0)
            {
                MoveLeft(-count);

            }

        }

        public void SetColumn(int col) 
        {
            if (col > Console.WindowWidth)
            {
                col = Console.WindowWidth;

            }

            if (col < 1)
            {
                col = 1;

            }

            terminal.Write($"{escapePrefix}{col}G");

            Column = col;

        }

        public void InsertSpace(int count = 1)
            => terminal.Write($"{escapePrefix}{count}@");

        public void DeleteCharacter(int count = 1)
        {
            terminal.Write($"{escapePrefix}{count}P");
            
            terminal.Buffer = terminal.buffer.Remove(Index, count);

        }
        
        public void ClearRemaining()
        {
            terminal.Write($"{escapePrefix}K");
        
            terminal.Buffer = terminal.buffer.Remove(Index);
        
        } 

        public void ClearScreen()
        {
            terminal.Write($"{escapePrefix}2J");

            terminal.buffer = string.Empty;

        }

        public void Write(string str)
        {
            if (str.StartsWith(escapePrefix))
            {
                Console.Write(str);

                return;

            }

            Column += str.Length;

            terminal.Buffer = terminal.buffer.Insert(Index - str.Length, str);

        }

        public void WriteLine(string str)
        {
            startingRow = GetAbsolutePosition().row;

            terminal.Write(str);

            SetPosition(
                terminal.buffer.Length / Console.WindowWidth + 1,
                terminal.buffer.Length % Console.WindowWidth + 1);
                
            Console.WriteLine();

            /* Uses a line based approach, so calling WriteLine resets all of
             *  the indexes: */
            Row = 1;
            Column = 1;
            terminal.buffer = string.Empty;

            startingRow = GetAbsolutePosition().row;

        }

        #endregion

    }

    #endregion

}