using System;

namespace Gomoku.Logic
{
  public class BoardChangedEventArgs : EventArgs
  {
    public readonly Player Player;
    public readonly Tile Tile;
    public readonly int Turn;

    public BoardChangedEventArgs(int turn, Player player, Tile tile)
    {
      Turn = turn;
      Player = player;
      Tile = tile;
    }
  }
}