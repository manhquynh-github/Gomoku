namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a tile in the board
  /// </summary>
  public class Tile
  {
    public Tile(int x, int y)
    {
      X = x;
      Y = y;
      Piece = new Piece(Pieces.None);
    }

    /// <summary>
    /// The <see cref="Piece"/> this <see cref="Tile"/> currently holds
    /// </summary>
    public Piece Piece { get; set; }

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