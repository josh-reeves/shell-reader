namespace ShellReader.Interfaces;

public interface IShellControls
{
    #region Properties
    IShellReader Reader { get; }

    #endregion

    #region Methods
    public string? Enter(string input);

    public string Backspace(string input);

    public string CtrlP(string input);

    public string CtrlN(string input);

    public string CtrlB(string input);

    public string CtrlF(string input);

    public string CtrlA(string input);

    public string CtrlE(string input);

    public string? CtrlC(string input);

    #endregion
    
}