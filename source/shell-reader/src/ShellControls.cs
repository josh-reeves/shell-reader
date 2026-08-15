using Interfaces;

namespace ShellReader;

public class ShellControls : IShellControls
{
    private int histIndex;

    public ShellControls(IShellReader reader)
    {
        histIndex = 0;

        Reader = reader;
        
        History = [];        
    
    }

    #region Properties
    public IShellReader Reader { get; set; }

    private IList<string> History { get; }

    #endregion

    #region Methods
    public string Enter(string input)
    {
        Reader.IsReading = false;
        
        return input;

    }

    public string Backspace(string input)
    {
        if (input.Length > 0)
        {
            input = input.Remove(input.Length - 1);
            Console.Write("\b \b");
        
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
        Console.Write(input);

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
        Console.Write(input);

        return input;
        
    }

    #endregion

}