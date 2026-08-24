using ShellReader.Interfaces;

namespace ShellReader;

public class ShellControls : IShellControls
{
    private int histIndex;

    public ShellControls(IShellReader reader)
    {
        Reader = reader;
        
        History = [];        
    
        histIndex = 0;

    }

    #region Properties
    public IShellReader Reader { get; set; }

    private IList<string> History { get; }

    #endregion

    #region Methods
    public string Enter(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            History.Add(input);

        }

        Reader.IsReading = false;

        histIndex = History.Count - 1;
        
        return input;

    }

    public string Backspace(string input)
    {
        if (input.Length > 0)
        {
            input = input.Remove(input.Length - 1);
            Reader.Terminal.Write("\b \b");
        
        }

        return input;

    }

    public string UpArrow(string input)
    {
        if (histIndex == History.Count - 1)
        {
            History[History.Count - 1] = input;
            
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
        return input;
        
    }

    public string RightArrow(string input)
    {
        Reader.Terminal.Cursor.MoveRight();

        return input;
        
    }

    public string Home(string input)
    {
        Reader.Terminal.Cursor.SetColumn(1 + Reader.Prompt.Length);
        
        return input;

    }

    public string CtrlA(string input)
        => Home(input);

    #endregion

}