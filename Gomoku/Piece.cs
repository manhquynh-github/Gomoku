using System.Windows.Media;

namespace Gomoku
{
    public class Piece : ViewModelBase
    {
        private string _symbol;
        private Brush _brush;

        public static readonly Piece EMPTY = new Piece(" ", null);
        public static readonly Piece X = new Piece("X", new SolidColorBrush(Color.FromRgb(255, 0, 0)));
        public static readonly Piece O = new Piece("O", new SolidColorBrush(Color.FromRgb(0, 0, 255)));

        public string Symbol
        {
            get => _symbol;
            private set
            {
                _symbol = value;
                NotifyPropertyChanged();
            }
        }
        public Brush Brush
        {
            get => _brush;
            private set
            {
                _brush = value;
                NotifyPropertyChanged();
            }
        }
                
        public Piece(string symbol, Brush brush)
        {
            Symbol = symbol;
            Brush = brush;
        }
    }
}
