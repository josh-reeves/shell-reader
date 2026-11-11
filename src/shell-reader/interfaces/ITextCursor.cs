namespace ShellReader.Interfaces;

public interface ITextCursor
{
    #region Properites
    public bool IsVirtual { get; set; }

    public int Column { get; }

    public int Row { get; }

    #endregion

    #region Methods
    public (int row, int col) GetPosition();

    public void SetPosition(int row, int col);

    public void MoveUp(int count = 1);
    
    public void MoveDown(int count = 1);
    
    /// <summary>
    /// Moves the cursor left by the specified number of positions without 
    /// wrapping.
    /// </summary>
    /// <param name="count">
    /// The number of positions to move the cursor to the left.
    /// </param>
    public void MoveLeft(int count = 1);
    
    /// <summary>
    /// Moves the cursor right by the specified number of positions without 
    /// wrapping.
    /// </summary>
    /// <param name="count">
    /// The number of positions to move the cursor to the right.
    /// </param>
    public void MoveRight(int count = 1);

    /// <summary>
    /// Advances the cursor position by the specified number of positions, 
    /// wrapping to the next line as necessary.
    /// </summary>
    /// <param name="count">
    /// The number of positions to shift the cursor. Positive values shift 
    /// the cursor right, negative values shift it to the left.
    /// </param>
    public void Advance(int count);

    public void SetColumn(int count);

    public void InsertSpace(int count = 1);

    public void DeleteCharacter(int count = 1);

    public void ClearRemaining();

    public void ClearScreen();

    #endregion

}