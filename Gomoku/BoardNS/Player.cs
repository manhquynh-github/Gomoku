namespace Gomoku.BoardNS
{
  /// <summary>
  /// Defines a player in the game.
  /// </summary>
  public class Player
  {
    public readonly Piece Piece;

    public Player(string name, Piece piece, bool isAuto = false)
    {
      Name = name;
      Piece = piece;
      IsAuto = isAuto;
    }

    /// <summary>
    /// If this <see cref="Player"/> will use AI.
    /// </summary>
    public bool IsAuto { get; set; }

    public string Name { get; set; }
  }
}