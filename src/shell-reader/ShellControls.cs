using ShellReader.Interfaces;

namespace ShellReader;

public class ShellControls : IShellControls
{
    #region Fields
    private int cursorIndex;
    private int historyIndex;
    private string buffer;
    
    private ITextCursor cursor => Reader.Terminal.Cursor;

    #endregion

    #region Constructor(s)
    public ShellControls(IShellReader reader)
    {
        cursorIndex = 0;
        historyIndex = 0;
        buffer = string.Empty;

        Reader = reader;
        Reader.CaptureText = UpdateTextAtCursor;

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

        cursorIndex = 0;
        historyIndex = History.Count - 1;
        buffer = string.Empty;

        Reader.IsReading = false;
        
        return input;

    }

    public string? CtrlC(string input)
        => Enter(null);
    
    public string? CtrlJ(string input)
        => Enter(input);

    // This works, but really needs to be cleaned up:
    public string Backspace(string input)
    {
        if (cursorIndex <= 0)
        {
            return input;

        }

        cursor.MoveLeft();
        cursor.DeleteCharacter();

        cursorIndex--;

        if (input.Length <= 0)
        {
            return input;

        }
                
        int charIndex = cursorIndex;

        if (Reader.IsPassword)
        {
            bool maskLength = Reader.Mask.Length > 0;    
            charIndex = maskLength ? charIndex / Reader.Mask.Length : input.Length;

        }

        if (charIndex > input.Length - 1) 
        { 
            charIndex = input.Length - 1; 
            
        }

        input = input.Remove(charIndex, 1);

        return input;

    }

    public string CtrlH(string input)
        => Backspace(input);

    public string CtrlD(string input)
    {
        Reader.Terminal.Cursor.DeleteCharacter();

        if (string.IsNullOrEmpty(input))
        {
            return input;

        }

        int col = Reader.Terminal.Cursor.Column - Reader.Prompt.Length,
                  charIndex = col - 1;
        
        if (Reader.IsPassword && Reader.Mask.Length > 0)
        {
            charIndex /= Reader.Mask.Length;
        
        }

        if (charIndex >= input.Length)
        {
            charIndex = input.Length - 1;

        }

        input = input.Remove(charIndex, 1);

        return input;

    }

    public string Del(string input)
        => CtrlD(input);

    public string CtrlL(string input)
        => input;

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

        Reader.ClearLine(Reader.Prompt.Length);
        Reader.Terminal.Write(input);

        cursorIndex = input.Length;
        buffer = input;

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

        Reader.ClearLine(Reader.Prompt.Length);
        Reader.Terminal.Write(input);

        cursorIndex = input.Length;
        buffer = input;

        return input;
        
    }

    public string DownArrow(string input)
        => CtrlN(input);

    public string CtrlB(string input)
    {
        if (cursorIndex > 0)
        {
            cursor.MoveLeft();
            cursorIndex--;

        }
        
        return input;
        
    }

    public string LeftArrow(string input)
        => CtrlB(input);

    public string CtrlF(string input)
    {
        int textLength = input.Length;

        if (Reader.IsPassword)
        {
            textLength *= Reader.Mask.Length;

        }

        if (cursorIndex < textLength)
        {
            Reader.Terminal.Cursor.MoveRight();
            cursorIndex++;

        }

        return input;
        
    }

    public string RightArrow(string input)
        => CtrlF(input);

    public string CtrlA(string input)
    {                
        Reader.Terminal.Cursor.SetColumn(Reader.Prompt.Length + 1);
        
        return input;

    }

    public string Home(string input)
        => CtrlA(input);

    public string CtrlE(string input)
    {
        int textLength = Reader.IsPassword ? input.Length * Reader.Mask.Length : input.Length;

        Reader.Terminal.Cursor.SetColumn(Reader.Prompt.Length + textLength + 1);

        return input;

    }

    public string End(string input)
        => CtrlE(input);

    public string UpdateTextAtCursor(string original, string insert)
    {
        int charIndex = cursorIndex;
        string displayText = insert;

        if (Reader.IsPassword)
        {
            displayText = Reader.Mask;
            charIndex = Reader.Mask.Length > 0 ? charIndex / Reader.Mask.Length : original.Length;
            
        }

        string updated = original.Insert(charIndex, insert);

        Reader.Terminal.Write(displayText + buffer[cursorIndex..]);

        cursor.MoveLeft(buffer[cursorIndex..].Length);

        buffer = buffer[..cursorIndex] + displayText + buffer[cursorIndex..];

        cursorIndex+= displayText.Length;

        return updated;
    
    }

    #endregion

}