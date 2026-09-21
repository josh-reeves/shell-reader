namespace ShellReader.Interfaces;

public interface IShellControls
{
    #region Properties
    IShellReader Reader { get; }

    #endregion

    #region Methods
    public string Enter(string input);

    public string Backspace(string input);

    public string Up(string input);

    public string CtrlP(string input);

    public string Down(string input);

    public string CtrlN(string input);

    public string Left(string input);

    public string CtrlB(string input);

    public string Right(string input);

    public string CtrlF(string input);

    public string Home(string input);

    public string CtrlA(string input);

    public string End(string input);

    public string CtrlE(string input);

    #endregion
    
}