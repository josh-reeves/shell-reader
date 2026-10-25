namespace ShellReader.Interfaces;

public interface IShellReader
{
    #region Properties
    public bool IsReading { get; set; }

    public bool IsPassword { get; }

    public bool InsertMode { get; set; }

    public string Prompt { get; }

    public string Mask { get; }

    public IConsole Terminal { get; set; }

    public Func<string, string, string> CaptureText { get; set; }

    public IDictionary<ConsoleKeyInfo, Func<string, string?>> KeyMap { get; }
    
    #endregion

    #region Events
    public event EventHandler<ConsoleKeyInfo> InputReceived;

    #endregion
    
    #region Methods
    public string? ReadPassword(string? prompt = null, string mask = "");

    public string? Read(string? prompt = null, bool isPassword = false);

    public void ClearLine(int startPos = 0);

    #endregion

}