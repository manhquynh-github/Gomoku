namespace Gomoku.BoardNS
{
    public class BoardChangingEventArgs
    {
        public readonly int Turn;
        public readonly Player Player;
        public readonly Tile Tile;

        public BoardChangingEventArgs(int turn, Player player, Tile tile)
        {
            Turn = turn;
            Player = player;
            Tile = tile;
        }
    }
}
