namespace ShellReader.Interfaces;

public interface IKeyHandler
{
    #region Properties
    public IDictionary<ConsoleKeyInfo, Func<string, string?>> KeyMap { get; }

    #endregion

    #region Methods
    public string? HandleKeyPress(string input);

    #endregion

}