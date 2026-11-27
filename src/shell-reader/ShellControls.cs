using ShellReader.Interfaces;

namespace ShellReader;

public class ShellControls : IShellControls
{
    #region Fields
    private int historyIndex;
    
    private int cursorIndex => cursor.Index - Reader.Prompt.Length;
    private string buffer => Reader.Terminal.Buffer as string ?? string.Empty; 
    private ITextCursor cursor => Reader.Terminal.Cursor;

    #endregion

    #region Constructor(s)
    public ShellControls(IShellReader reader)
    {
        historyIndex = 0;

        Reader = reader;

        History = [string.Empty];        
    
    }

    #endregion

    #region Properties
    public IShellReader Reader { get; set; }

    private IList<string> History { get; } 

    #endregion

    #region Methods
    public virtual string? Enter(string? input)
    {
        if (!string.IsNullOrWhiteSpace(input) && !Reader.IsPassword)
        {
            History[History.Count - 1] = input;
            History.Add(string.Empty);

        }

        historyIndex = History.Count - 1;

        Reader.IsReading = false;
        
        return input;

    }

    public virtual string? CtrlJ(string input)
        => Enter(input);

    public virtual string? CtrlC(string input)
        => Enter(null);
    
    public virtual string? CtrlZ(string input)
        => CtrlC(input);

    public virtual string Backspace(string input)
    {
        if (cursorIndex > 0)
        {
            cursor.Advance(-1);
            cursor.DeleteCharacter();

        }

        if (string.IsNullOrEmpty(input) || cursorIndex <= 0)
        {
            return input;

        }
                
        if (Reader.IsPassword && Reader.Mask.Length <= 0)
        {
            input = input[..^1];

            return input;

        }

        if (cursorIndex < input.Length) 
        { 
            input = input.Remove(cursorIndex, 1);
            
        }

        return input;

    }

    public virtual string CtrlH(string input)
        => Backspace(input);

    public virtual string CtrlD(string input)
    {
        if (cursorIndex >= 0 && cursorIndex < input.Length)
        {
            cursor.DeleteCharacter();

        }

        if (string.IsNullOrEmpty(input))
        {
            return input;

        }

        if (cursorIndex < input.Length)
        {
            input = input.Remove(cursorIndex, 1);

        }

        return input;

    }

    public virtual string Del(string input)
        => CtrlD(input);

    public virtual string CtrlL(string input)
    {
        cursor.ClearScreen();
        cursor.SetPosition(1, 1);

        Reader.Terminal.Write(Reader.Prompt + input);

        return input;

    }

    public virtual string CtrlP(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        if (historyIndex > 0)
        {
            historyIndex--;

            input = History[historyIndex];            

        }

        input = Home(input);
        cursor.ClearRemaining();

        Reader.Terminal.Write(input);

        return input;

    }

    public virtual string UpArrow(string input)
        => CtrlP(input);

    public virtual string CtrlN(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        if (historyIndex < History.Count - 1)
        {
            historyIndex++;

            input = History[historyIndex];
            
        }

        input = Home(input);
        cursor.ClearRemaining();

        Reader.Terminal.Write(input);

        return input;
        
    }

    public virtual string DownArrow(string input)
        => CtrlN(input);

    public virtual string CtrlB(string input)
    {
        if (cursorIndex > 0)
        {
            cursor.Advance(-1);

        }
        
        return input;
        
    }

    public virtual string LeftArrow(string input)
        => CtrlB(input);

    public virtual string CtrlF(string input)
    {
        if (cursor.Index < buffer.Length)
        {
            cursor.Advance(1);

        }

        return input;
        
    }

    public virtual string RightArrow(string input)
        => CtrlF(input);

    public virtual string CtrlA(string input)
    {                
        cursor.SetPosition(1, 1 + Reader.Prompt.Length);
        
        return input;

    }

    public virtual string Home(string input)
        => CtrlA(input);

    public virtual string CtrlE(string input)
    {
        cursor.SetPosition(
            buffer.Length / Console.WindowWidth + 1, 
            buffer.Length % Console.WindowWidth + 1);

        return input;

    }

    public virtual string End(string input)
        => CtrlE(input);

    public virtual string Insert(string input)
    {
        Reader.InsertMode = !Reader.InsertMode;

        return input;

    }

    public virtual string F3(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        if (historyIndex > 0)
        {
            historyIndex--;

            input = History[historyIndex];

            input = Home(input);
            cursor.ClearRemaining();

            Reader.Terminal.Write(input);
            
        }

        return input;

    }

    public virtual string AltF(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        foreach (string sep in Reader.Separators)
        {
            int index = buffer.IndexOf(sep, cursor.Index);

            if (index >= 0)
            {
                cursor.SetPosition(
                    index / Console.WindowWidth + 1, 
                    index % Console.WindowWidth + Reader.Prompt.Length);

                break;

            }

        }
        
        return input;
        
    }

    public string AltB(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        foreach (string sep in Reader.Separators)
        {
            int index = buffer.LastIndexOf(sep, cursor.Index - 1);

            if (index >= 0)
            {
                cursor.SetPosition(
                    index / Console.WindowWidth + 1, 
                    index % Console.WindowWidth + Reader.Prompt.Length);
                                    
                break;

            }

        }
        
        return input;
        
    }

    public void RegisterAllWithDefaults(IShellReader? reader = null)
    {
        reader ??= Reader;

        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false), Enter);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false), Backspace);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false), UpArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false), DownArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false), LeftArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false), RightArrow);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false), Home);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.A, false, false, true), CtrlA);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false), End);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.C, false, false, true), CtrlC);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.D, false, false, true), CtrlD);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false), Del);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.F, false, false, true), CtrlF);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.H, false, false, true), CtrlH);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.J, false, false, true), CtrlJ);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.L, false, false, true), CtrlL);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.N, false, false, true), CtrlN);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.P, false, false, true), CtrlP);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.B, false, false, true), CtrlB);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Z, false, false, true), CtrlZ);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Insert, false, false, false), Insert);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.F3, false, false, false), F3);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.F, false, true, false), AltF);
        reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.B, false, true, false), AltB);
        
    }

    #endregion

}