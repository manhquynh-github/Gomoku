using Gomoku.AI;

namespace Gomoku.BoardNS
{
  /// <summary>
  /// Defines a player in the game.
  /// </summary>
  public class Player
  {
    public readonly Piece Piece;

    public Player(string name, Piece piece, AbstractGomokuAI ai, bool isAuto = false)
    {
      Name = name;
      Piece = piece;
      AI = ai;
      IsAuto = isAuto;
    }

    public AbstractGomokuAI AI { get; }

    /// <summary>
    /// If this <see cref="Player"/> will use AI.
    /// </summary>
    public bool IsAuto { get; set; }

    public string Name { get; set; }
  }
}