using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Board
{
    public class GameOverEventArgs
    {
        public readonly Board Board;
        public readonly bool IsGameOver;
        public readonly int Turn;
        public readonly Player Winner;

        public GameOverEventArgs(Board board, bool isGameOver, int turn, Player winner)
        {
            Board = board;
            IsGameOver = isGameOver;
            Turn = turn;
            Winner = winner;
        }
    }
}
