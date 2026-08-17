namespace ShellReader.Interfaces;

public interface IConsole
{
    #region Properties
    public ITextCursor Cursor { get; }

    #endregion

    #region Methods
    public ConsoleKeyInfo ReadKey(bool intercept);

    public void Write(object? value = null);

    public void WriteLine(object? value = null);

    #endregion
    
}