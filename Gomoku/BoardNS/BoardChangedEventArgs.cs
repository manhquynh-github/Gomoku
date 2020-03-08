using System;

namespace Gomoku.BoardNS
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