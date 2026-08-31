namespace ShellReader.Interfaces;

public interface IShellControls
{
    #region Properties
    IShellReader Reader { get; }

    #endregion

    #region Methods
    public string Enter(string input);

    public string Backspace(string input);

    public string UpArrow(string input);

    public string DownArrow(string input);

    public string LeftArrow(string input);

    public string RightArrow(string input);

    public string Home(string input);

    public string CtrlA(string input);

    public string End(string input);

    #endregion
    
}