using System;

namespace Gomoku.Board
{
    public class TurnChangedEventArgs : EventArgs
    {
        public readonly Board Board;
        public readonly int Turn;
        public readonly Player Player;

        public TurnChangedEventArgs(Board board, int turn, Player player)
        {
            Board = board;
            Turn = turn;
            Player = player;
        }
    }
}
