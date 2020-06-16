using System;

namespace Gomoku.Logic
{
  public class GameOverEventArgs
  {
    public GameOverEventArgs(bool isGameOver, int turn, Player winner, Line winningLine)
    {
      IsGameOver = isGameOver;
      Turn = turn;
      Winner = winner ?? throw new ArgumentNullException(nameof(winner));
      WinningLine = winningLine ?? throw new ArgumentNullException(nameof(winningLine));
    }

    public bool IsGameOver { get; }
    public int Turn { get; }
    public Player Winner { get; }
    public Line WinningLine { get; }
  }
}