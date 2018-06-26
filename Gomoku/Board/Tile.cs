using System.Windows;

namespace Gomoku.Board
{
    public class Tile : ViewModelBase
    {
        private Piece _piece;
        public int X { get; set; }
        public int Y { get; set; }
        public UIElement UIElement { get; set; }

        public Piece Piece
        {
            get => _piece;
            set
            {
                _piece = value;
                NotifyPropertyChanged();
            }
        }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Piece = Piece.EMPTY;
        }           
    }
}
