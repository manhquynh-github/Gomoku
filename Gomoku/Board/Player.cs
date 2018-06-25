using System;

namespace Gomoku.Board
{
    public class Player
    {
        public String Name { get; set; }
        public readonly Piece Piece;

        public Player(string name, Piece piece)
        {
            Name = name;
            Piece = piece;
        }
    }
}
