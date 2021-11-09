using Gomoku.Logic.AI;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a player in the game.
  /// </summary>
  public class Player
  {
    public Player(string name, Piece piece, GomokuAIBase ai, bool isAuto = false)
    {
      Name = name;
      Piece = piece;
      AI = ai;
      IsAuto = isAuto;
    }

    /// <summary>
    /// The AI used for this player.
    /// </summary>
    public GomokuAIBase AI { get; set; }

    /// <summary>
    /// If this <see cref="Player"/> will use AI.
    /// </summary>
    public bool IsAuto { get; set; }

    /// <summary>
    /// Name of player.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The piece that this player will have
    /// </summary>
    public Piece Piece { get; }
  }
}