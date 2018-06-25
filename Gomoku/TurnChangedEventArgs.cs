using System;

namespace Gomoku
{
    public class TurnChangedEventArgs : EventArgs
    {
        public readonly int order;
        public readonly Piece piece;

        public TurnChangedEventArgs(int order, Piece piece)
        {
            this.order = order;
            this.piece = piece;
        }
    }
}
