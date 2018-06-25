using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gomoku
{
    public class Tile : INotifyPropertyChanged
    {
        private Piece _piece;

        public int X { get; set; }
        public int Y { get; set; }
        public Piece Piece
        {
            get => _piece;
            set
            {
                _piece = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Piece = Piece.EMPTY;
        }           
    }
}
