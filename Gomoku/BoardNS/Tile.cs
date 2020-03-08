namespace Gomoku.BoardNS
{
  /// <summary>
  /// Defines a tile in the board
  /// </summary>
  public class Tile : VMBase
  {
    private bool _isHighlighted;
    private Piece _piece;

    public Tile(int x, int y)
    {
      X = x;
      Y = y;
      Piece = new Piece(Pieces.None);
    }

    public bool IsHighlighted
    {
      get => _isHighlighted;
      set => SetProperty(ref _isHighlighted, value);
    }

    /// <summary>
    /// The <see cref="Piece"/> this <see cref="Tile"/> currently holds
    /// </summary>
    public Piece Piece
    {
      get => _piece;
      set => SetProperty(ref _piece, value);
    }

    /// <summary>
    /// The X coordinate of the <see cref="Tile"/>
    /// </summary>
    public int X { get; }

    /// <summary>
    /// The Y coordinate of the <see cref="Tile"/>
    /// </summary>
    public int Y { get; }
  }
}