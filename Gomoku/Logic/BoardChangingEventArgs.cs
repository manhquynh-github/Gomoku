namespace Gomoku.Logic
{
  public class BoardChangingEventArgs
  {
    public readonly Player Player;
    public readonly Tile Tile;
    public readonly int Turn;

    public BoardChangingEventArgs(int turn, Player player, Tile tile)
    {
      Turn = turn;
      Player = player;
      Tile = tile;
    }
  }
}