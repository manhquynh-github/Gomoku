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
using Gomoku.Board;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly Board.Board Board;

        public MainWindow() :
            this(15, 15,
                new List<Player>()
                {
                    new Player("Player 1", Piece.X),
                    new Player("Player 2", Piece.O, true)
                })
        {
        }

        public MainWindow(int boardWidth, int boardHeight, IList<Player> players)
        {
            InitializeComponent();

            Board = new Board.Board(boardWidth, boardHeight, players);
            InitializeBoard(boardWidth, boardHeight);
        }

        private void InitializeBoard(int width, int height)
        {
            Style widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;
            Style tileStyle = Resources["TileButtonStyle"] as Style;

            StackPanel columnStackPanel = new StackPanel();
            columnStackPanel.Style = widthStackPanelStyle;
            for (int j = -1; j < width; j++)
            {
                Button tileButton = new Button();
                tileButton.Style = tileStyle;
                tileButton.Content = j;
                columnStackPanel.Children.Add(tileButton);
            }
            HeightStackPanel.Children.Add(columnStackPanel);

            for (int i = 0; i < height; i++)
            {
                StackPanel widthStackPanel = new StackPanel();
                widthStackPanel.Style = widthStackPanelStyle;

                Button rowButton = new Button();
                rowButton.Style = tileStyle;
                rowButton.Content = i;
                widthStackPanel.Children.Add(rowButton);

                for (int j = 0; j < width; j++)
                {
                    Button tileButton = new Button();
                    tileButton.DataContext = Board.Tiles[j, i];
                    tileButton.Style = tileStyle;
                    widthStackPanel.Children.Add(tileButton);
                }

                HeightStackPanel.Children.Add(widthStackPanel);
            }

            Board.GameOver += Board_GameOver;
        }

        private void Board_GameOver(GameOverEventArgs e)
        {
            MessageBox.Show(e.Winner.Name + " wins!");
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.DataContext != null)
            {
                Board.Play(button.DataContext as Tile);
                button.Content = (button.DataContext as Tile).Piece.Symbol;

                // AI
                if (Board.GetCurrentPlayer().IsAuto && UseAI.IsChecked == true)
                {
                    Board.Play(new Gomoku.AI.GomokuAIv1().Play(Board));
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Board.Play(new Gomoku.AI.GomokuAIv1().Play(Board));
        }
    }
}
