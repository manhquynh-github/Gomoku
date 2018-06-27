using System;

namespace Gomoku.Board
{
    public class BoardChangedEventArgs : EventArgs
    {
        public readonly int Turn;
        public readonly Player Player;
        public readonly Tile Tile;

        public BoardChangedEventArgs(int turn, Player player, Tile tile)
        {
            Turn = turn;
            Player = player;
            Tile = tile;
        }
    }
}
