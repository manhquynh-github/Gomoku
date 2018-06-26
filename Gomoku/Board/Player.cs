using System;

namespace Gomoku.Board
{
    public class Player
    {
        public String Name { get; set; }
        public readonly Piece Piece;
        public bool IsAuto { get; set; }

        public Player(string name, Piece piece, bool isAuto = false)
        {
            Name = name;
            Piece = piece;
            IsAuto = isAuto;
        }
    }
}
