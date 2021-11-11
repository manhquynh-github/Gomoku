using System;
using System.Collections.Generic;

namespace Gomoku.Logic
{
  /// <summary>
  /// Represents the objects related to <see cref="Game.GameOver"/> event.
  /// </summary>
  public class GameOverEventArgs
  {
    public GameOverEventArgs(
      int turn,
      Player winner,
      IList<Tile> winningTiles)
    {
      Turn = turn;
      Winner = winner;
      WinningTiles = winningTiles
        ?? throw new ArgumentNullException(nameof(winningTiles));
    }

    public int Turn { get; }
    public Player Winner { get; }
    public IList<Tile> WinningTiles { get; }
  }
}