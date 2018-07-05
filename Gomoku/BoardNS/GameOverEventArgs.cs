using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.BoardNS
{
    public class GameOverEventArgs
    {
        public readonly bool IsGameOver;
        public readonly int Turn;
        public readonly Player Winner;

        public GameOverEventArgs(bool isGameOver, int turn, Player winner)
        {
            IsGameOver = isGameOver;
            Turn = turn;
            Winner = winner;
        }
    }
}
