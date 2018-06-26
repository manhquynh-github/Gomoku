using System;

namespace Gomoku.Board
{
    /// <summary>
    /// Defines a player in the game.
    /// </summary>
    public class Player
    {
        public String Name { get; set; }
        public readonly Piece Piece;
        /// <summary>
        /// If this <see cref="Player"/> will use AI.
        /// </summary>
        public bool IsAuto { get; set; }

        public Player(string name, Piece piece, bool isAuto = false)
        {
            Name = name;
            Piece = piece;
            IsAuto = isAuto;
        }
    }
}
