namespace Interfaces;

public interface IConsole
{
    #region Properties
    public ITextCursor Cursor { get; }

    #endregion

    #region Methods
    public ConsoleKeyInfo ReadKey(bool intercept);

    public void Write(string value);

    public void WriteLine(string value);

    #endregion
    
}