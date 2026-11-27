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
    public string? Enter(string? input)
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

    public string? CtrlJ(string input)
        => Enter(input);

    public string? CtrlC(string input)
        => Enter(null);
    
    public string? CtrlZ(string input)
        => CtrlC(input);

    public string Backspace(string input)
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

    public string CtrlH(string input)
        => Backspace(input);

    public string CtrlD(string input)
    {
        if (buffer.Length > Reader.Prompt.Length)
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

    public string Del(string input)
        => CtrlD(input);

    public string CtrlL(string input)
    {
        cursor.ClearScreen();
        cursor.SetPosition(1, 1);

        Reader.Terminal.Write(Reader.Prompt + input);

        return input;

    }

    public string CtrlP(string input)
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

    public string UpArrow(string input)
        => CtrlP(input);

    public string CtrlN(string input)
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

    public string DownArrow(string input)
        => CtrlN(input);

    public string CtrlB(string input)
    {
        if (cursorIndex > 0)
        {
            cursor.Advance(-1);

        }
        
        return input;
        
    }

    public string LeftArrow(string input)
        => CtrlB(input);

    public string CtrlF(string input)
    {
        if (cursor.Index < buffer.Length)
        {
            cursor.Advance(1);

        }

        return input;
        
    }

    public string RightArrow(string input)
        => CtrlF(input);

    public string CtrlA(string input)
    {                
        cursor.SetPosition(1, 1 + Reader.Prompt.Length);
        
        return input;

    }

    public string Home(string input)
        => CtrlA(input);

    public string CtrlE(string input)
    {
        cursor.SetPosition(
            buffer.Length / Console.WindowWidth + 1, 
            buffer.Length % Console.WindowWidth + 1);

        return input;

    }

    public string End(string input)
        => CtrlE(input);

    public string Insert(string input)
    {
        Reader.InsertMode = !Reader.InsertMode;

        return input;

    }

    public string F3(string input)
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

    public string AltF(string input)
    {
        foreach (string sep in Reader.Separators)
        {
            int index = buffer.IndexOf(sep, cursorIndex);

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
        foreach (string sep in Reader.Separators)
        {
            int index = buffer.LastIndexOf(sep, cursorIndex);

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

    #endregion

}