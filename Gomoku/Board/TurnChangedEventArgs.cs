using System;

namespace Gomoku.Board
{
    public class TurnChangedEventArgs : EventArgs
    {
        public readonly Board Board;
        public readonly int Order;
        public readonly Player Player;

        public TurnChangedEventArgs(Board board, int order, Player player)
        {
            Board = board;
            Order = order;
            Player = player;
        }
    }
}
