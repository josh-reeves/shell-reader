namespace ShellReader.Interfaces;

public interface ITextCursor
{
    #region Properites
    public int Column { get; }

    public int Row { get; }

    #endregion

    #region Methods
    public void MoveUp(int count = 1);
    
    public void MoveDown(int count = 1);

    public void MoveLeft(int count = 1);
    
    public void MoveRight(int count = 1);
    
    public void SetColumn(int count);

    public void ClearRemaining();

    public void SetPosition(int row, int col);

    public (int row, int col) GetPosition();

    #endregion

}