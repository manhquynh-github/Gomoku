using System.Windows;

namespace Gomoku.Board
{
    /// <summary>
    /// Defines a tile in the board
    /// </summary>
    public class Tile : ViewModelBase
    {
        private Piece _piece;
        /// <summary>
        /// The X coordinate of the <see cref="Tile"/>
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// The Y coordinate of the <see cref="Tile"/>
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The <see cref="Piece"/> this <see cref="Tile"/> currently holds
        /// </summary>
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
