namespace Interfaces;

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

    #endregion
    
}