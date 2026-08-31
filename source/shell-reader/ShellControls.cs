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
    public string Enter(string input)
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

    public string Backspace(string input)
    {
        if (input.Length <= 0)
        {
            return input;

        }

        int col = Reader.Terminal.Cursor.Column,
            offset = 1,
            charIndex = col - 1 - Reader.Prompt.Length - 1;

        if (Reader.IsPassword && Reader.Mask.Length > 0)
        {
            offset = Reader.Mask.Length - ((col - 1 - Reader.Prompt.Length) % Reader.Mask.Length);

            charIndex /= Reader.Mask.Length;

        }
        else if (Reader.IsPassword && input.Length > 0)
        {
            input = input[..(input.Length - 1)];

        }

        string backspaces = "\b";

        for (int i = 1; i < offset; i++)
        {
            backspaces += "\b";

        }

        if (charIndex >= 0)
        {
            input = input.Remove(charIndex, 1);
            Reader.Terminal.Write(backspaces);
            Reader.Terminal.Cursor.DeleteCharacter(offset);
        
        }

        return input;

    }

    public string UpArrow(string input)
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

    public string DownArrow(string input)
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

    public string LeftArrow(string input)
    {
        int col = Reader.Terminal.Cursor.Column;

        if (col > Reader.Prompt.Length + 1)
        {
            Reader.Terminal.Cursor.MoveLeft();

        }
        
        return input;
        
    }

    public string RightArrow(string input)
    {
        int col = Reader.Terminal.Cursor.Column,
            textLength = Reader.IsPassword ? input.Length * Reader.Mask.Length : input.Length;

        if (col < Reader.Prompt.Length + textLength + 1)
        {
            Reader.Terminal.Cursor.MoveRight();

        }

        return input;
        
    }

    public string Home(string input)
    {                
        Reader.Terminal.Cursor.SetColumn(Reader.Prompt.Length + 1);
        
        return input;

    }

    public string CtrlA(string input)
        => Home(input);

    public string End(string input)
    {
        int textLength = Reader.IsPassword ? input.Length * Reader.Mask.Length : input.Length;

        Reader.Terminal.Cursor.SetColumn(Reader.Prompt.Length + textLength + 1);

        return input;

    }

    #endregion

}