using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku
{
    [Flags]
    public enum Direction
    {
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8,
        UpLeft = Left | Up,
        UpRight = Right | Up,
        DownLeft = Left | Down,
        DownRight = Right | Down,
        All = Left | Up | Right | Down
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        Diagonal,
        RvDiagonal
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }
}
