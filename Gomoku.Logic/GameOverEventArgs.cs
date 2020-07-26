using System;
using System.Collections.Generic;

namespace Gomoku.Logic
{
  public class GameOverEventArgs
  {
    public GameOverEventArgs(
      bool isGameOver,
      int turn,
      Player winner,
      IEnumerable<Tile> winningTiles)
    {
      IsGameOver = isGameOver;
      Turn = turn;
      Winner = winner
        ?? throw new ArgumentNullException(nameof(winner));
      WinningTiles = winningTiles
        ?? throw new ArgumentNullException(nameof(winningTiles));
    }

    public bool IsGameOver { get; }
    public int Turn { get; }
    public Player Winner { get; }
    public IEnumerable<Tile> WinningTiles { get; }
  }
}