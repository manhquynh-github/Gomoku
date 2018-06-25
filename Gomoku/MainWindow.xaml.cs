using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly Board Board;

        public MainWindow() :
            this(15, 15,
                new List<Player>()
                {
                    new Player("Player 1", Piece.X),
                    new Player("Player 2", Piece.O)
                })
        {
        }

        public MainWindow(int boardWidth, int boardHeight, IList<Player> players)
        {
            InitializeComponent();

            Board = new Board(boardWidth, boardHeight, players);
            InitializeBoard(boardWidth, boardHeight);
        }

        private void InitializeBoard(int width, int height)
        {
            Style widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;
            Style tileStyle = Resources["TileButtonStyle"] as Style;

            for (int i = 0; i < height; i++)
            {
                StackPanel widthStackPanel = new StackPanel();
                widthStackPanel.Style = widthStackPanelStyle;
                for (int j = 0; j < width; j++)
                {
                    Button tileButton = new Button();
                    tileButton.DataContext = Board.Tiles[j, i];
                    tileButton.Style = tileStyle;
                    widthStackPanel.Children.Add(tileButton);
                }

                HeightStackPanel.Children.Add(widthStackPanel);
            }
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.DataContext != null)
            {
                Board.Play(button.DataContext as Tile);
                button.Content = (button.DataContext as Tile).Piece.Symbol;
            }
        }
    }
}
