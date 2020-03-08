using System;
using System.Collections.Generic;

namespace Gomoku.BoardNS
{
  public class GameOverEventArgs
  {
    public bool IsGameOver { get; }
    public int Turn { get; }
    public Player Winner { get; }
    public Line WinningLine { get; }

    public GameOverEventArgs(bool isGameOver, int turn, Player winner, Line winningLine)
    {
      IsGameOver = isGameOver;
      Turn = turn;
      Winner = winner ?? throw new ArgumentNullException(nameof(winner));
      WinningLine = winningLine ?? throw new ArgumentNullException(nameof(winningLine));
    }
  }
}