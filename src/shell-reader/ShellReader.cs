using System.Runtime.InteropServices;
using ShellReader.Interfaces;

namespace ShellReader;

public class ShellReader : IShellReader
{
    #region Fields
    private ITextCursor cursor => Terminal.Cursor;

    #endregion

    public ShellReader(string prompt = "", string mask = "", IConsole? terminal = null, IDictionary<ConsoleKeyInfo, Func<string, string?>>? keyMap = null)
    {
        Mask = mask;

        Prompt = prompt;
        
        KeyMap = keyMap ?? new Dictionary<ConsoleKeyInfo, Func<string, string?>>();

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

    public string Mask { get; private set; }

    public IConsole Terminal { get; set; }

    public IDictionary<ConsoleKeyInfo, Func<string, string?>> KeyMap { get; }

    #endregion

    #region Methods
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

    private string UpdateTextAtCursor(string original, string insert)
    {
        int col = Terminal.Cursor.Column,
            adjusted = col - Prompt.Length,
            textLength = (IsPassword && Mask.Length <= 0) ? 0 : original.Length;
        
        string updated = original;

        if (adjusted <= textLength)
        {
            updated = original.Insert(adjusted - 1, insert);
        
        }
        else
        {
            updated += insert;
            
        }

        insert = IsPassword ? Mask : insert;

        Terminal.Cursor.InsertSpace(insert.Length);
        Terminal.Write(insert);
    
        return updated;

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
    private const int STDINFILE = 0;
    private const int TCSANOW = 0;
    private const uint ICANON = 0x00000002;
    private const uint ECHO = 0x00000008;

    private readonly TextCursor cursor;

    #endregion

    #region Constructor(s)
    public Terminal()
    {        
        cursor = new TextCursor(this);

    }

    #endregion

    #region Properties
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
        private const char Newline = '\n',
                           Escape = '\u001B';

        private (int row, int col) virtualCursor;
        private string escapePrefix => $"{Escape}[";

        private Terminal terminal;

        #endregion

        #region Constructor(s)
        public TextCursor(IConsole parent)
        {
            terminal = (Terminal)parent;
            virtualCursor = (1, 1);

            IsVirtual = Console.IsInputRedirected;
            
        }

        #endregion

        #region Properties
        public bool IsVirtual { get; set; }

        public int Column { get => GetPosition().col; }

        public int Row { get => GetPosition().row; }

        #endregion

        #region Methods
        private (int row, int col) GetPositionLinux()
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
                return (virtualCursor.row, virtualCursor.col);

            }
            finally
            {
                // Restore original terminal mode:
                TCSetAttr(STDINFILE, TCSANOW, ref original);
                
            }

        }

        public (int row, int col) GetPosition()
        {
            if (IsVirtual)
            {
                return (virtualCursor.row, virtualCursor.col);
                
            }

            if (OperatingSystem.IsLinux())
            {
                return GetPositionLinux();

            }

            return (Console.CursorTop + 1, Console.CursorLeft + 1);

        }

        public void MoveUp(int count = 1) 
        {
            terminal.Write($"{escapePrefix}{count}A");
        
            virtualCursor.col--;

        }

        public void MoveDown(int count = 1) 
        {
            terminal.Write($"{escapePrefix}{count}B");

            virtualCursor.col++;
            
        }

        public void MoveLeft(int count = 1)
        {
            terminal.Write($"{escapePrefix}{count}D");

            virtualCursor.col--;

        }
            
        
        public void MoveRight(int count = 1) 
        {
            terminal.Write($"{escapePrefix}{count}C");

            virtualCursor.col++;

        }
        
        public void SetColumn(int col) 
        {
            terminal.Write($"{escapePrefix}{col}G");

            virtualCursor.col = col;

        }

        public void InsertSpace(int count = 1)
            => terminal.Write($"{escapePrefix}{count}@");

        public void DeleteCharacter(int count = 1)
            => terminal.Write($"{escapePrefix}{count}P");
        
        public void ClearRemaining() 
            => terminal.Write($"{escapePrefix}K");

        public void SetPosition(int row, int col) 
        {
            terminal.Write($"{escapePrefix}{row};{col}H");

            virtualCursor.row = row;
            virtualCursor.col = col;

        }

        public void Write(string str)
        {
            Console.Write(str);

            if (!str.StartsWith(escapePrefix))
            {
                virtualCursor.col += str.Length;

            }

            if (str.Contains(Newline))
            {
                virtualCursor.row += str.Count((c) => c == Newline);
                virtualCursor.col = str.Length - str.LastIndexOf(Newline);

            }

        }

        public void WriteLine(string str)
        {
            Console.WriteLine(str);

            virtualCursor.row += str.Count((c) => c == Newline) + 1;
            virtualCursor.col = 1;

        }

        #endregion

    }

    #endregion

}