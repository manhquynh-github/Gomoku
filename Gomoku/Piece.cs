using System;
using System.Windows.Media;

namespace Gomoku
{
    public class Piece
    {
        public readonly char Symbol;
        public readonly Brush Brush;

        public static readonly Piece EMPTY = new Piece(' ', null);
        public static readonly Piece X = new Piece('X', new SolidColorBrush(Color.FromRgb(255, 0, 0)));
        public static readonly Piece O = new Piece('O', new SolidColorBrush(Color.FromRgb(0, 0, 255)));

        public Piece(char symbol, Brush brush)
        {
            Symbol = symbol;
            Brush = brush;
        }
    }
}
