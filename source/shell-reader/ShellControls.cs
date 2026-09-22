using ShellReader.Interfaces;

namespace ShellReader;

public class ShellControls : IShellControls
{
    private int histIndex;

    public ShellControls(IShellReader reader)
    {
        Reader = reader;
        
        History = [string.Empty];        
    
        histIndex = 0;

    }

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

        histIndex = History.Count - 1;

        Reader.IsReading = false;
        
        return input;

    }

    // This works, but really needs to be cleaned up:
    public string Backspace(string input)
    {
        if (input.Length <= 0)
        {
            return input;
            
        }

        if (Reader.IsPassword && Reader.Mask.Length <= 0)
        {
            input = input[..(input.Length - 1)];

            return input;

        }

        int col = Reader.Terminal.Cursor.Column,
            cols = 1,
            cursorOffset = 1,
            adjusted = col - Reader.Prompt.Length,
            charIndex = adjusted - cursorOffset - 1;

        if (charIndex < 0)
        {
            return input;
            
        }

        if (Reader.IsPassword)
        {
            charIndex /= Reader.Mask.Length;
            cols *= Reader.Mask.Length;

            cursorOffset = adjusted % Reader.Mask.Length;
            
        }

        input = input.Remove(charIndex, 1);
        Reader.Terminal.Cursor.SetColumn(col - cursorOffset + 1 - cols);
        Reader.Terminal.Cursor.DeleteCharacter(cols);

        return input;

    }
    public string CtrlP(string input)
    {
        if (Reader.IsPassword)
        {
            return input;

        }

        if (histIndex > 0)
        {
            histIndex--;

            input = History[histIndex];            

        }

        Reader.ClearLine(Reader.Prompt.Length);
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

        if (histIndex < History.Count - 1)
        {
            histIndex++;

            input = History[histIndex];
            
        }

        Reader.ClearLine(Reader.Prompt.Length);
        Reader.Terminal.Write(input);

        return input;
        
    }

    public string DownArrow(string input)
        => CtrlN(input);

    public string CtrlB(string input)
    {
        int col = Reader.Terminal.Cursor.Column;

        if (col > Reader.Prompt.Length + 1)
        {
            Reader.Terminal.Cursor.MoveLeft();

        }
        
        return input;
        
    }

    public string LeftArrow(string input)
        => CtrlB(input);

    public string CtrlF(string input)
    {
        int col = Reader.Terminal.Cursor.Column,
            textLength = Reader.IsPassword ? input.Length * Reader.Mask.Length : input.Length;

        if (col < Reader.Prompt.Length + textLength + 1)
        {
            Reader.Terminal.Cursor.MoveRight();

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

    public string? CtrlC(string? input)
        => Enter(null);

    #endregion

}