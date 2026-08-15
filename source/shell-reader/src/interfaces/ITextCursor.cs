namespace Interfaces;

public interface ITextCursor
{
    #region Methods
    public void MoveUp(int count = 1);
    
    public void MoveDown(int count = 1);

    public void MoveLeft(int count = 1);
    
    public void MoveRight(int count = 1);
    
    public void SetColumn(int count);

    public void ClearRemaining();
            
    public (int row, int col) GetPosition();

    public void SetPosition(int row, int col);

    #endregion

}