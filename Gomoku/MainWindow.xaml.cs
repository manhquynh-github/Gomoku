using Gomoku.AI;
using Gomoku.BoardNS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly AbstractGomokuAI AI;
        public readonly Board Board;
        public readonly Dictionary<Tile, Button> Buttons;
        private IEnumerable<Tile> Choices;

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

            Board = new Board(boardWidth, boardHeight, players);
            Buttons = new Dictionary<Tile, Button>();
            InitializeBoard(boardWidth, boardHeight);
            AI = new GomokuAIv1();
            Choices = new List<Tile>();
        }

        private async Task<Tile> AIPlayAsync()
        {
            var result = await AI.PlayAsync(Board);
            await Task.Delay(500);
            Choices = result.Item2;
            return result.Item1;
        }

        private async Task RunAI()
        {
            Tile tile = await AIPlayAsync();
            if (tile == null)
                return;

            TileButton_Click(Buttons[tile], null);
        }

        private void CleanAnalyze()
        {
            if (Choices != null && Choices.Count() > 0)
                foreach (var tile in Choices)
                {
                    Buttons[tile].BorderThickness = new Thickness(1.0);
                }
        }

        private void InitializeBoard(int width, int height)
        {
            Style widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;
            Style tileStyle = Resources["TileButtonStyle"] as Style;

            StackPanel columnStackPanel = new StackPanel
            {
                Style = widthStackPanelStyle
            };
            for (int j = -1; j < width; j++)
            {
                Button tileButton = new Button
                {
                    Style = tileStyle,
                    Content = j
                };
                columnStackPanel.Children.Add(tileButton);
            }
            HeightStackPanel.Children.Add(columnStackPanel);

            for (int i = 0; i < height; i++)
            {
                StackPanel widthStackPanel = new StackPanel
                {
                    Style = widthStackPanelStyle
                };

                Button rowButton = new Button
                {
                    Style = tileStyle,
                    Content = i
                };
                widthStackPanel.Children.Add(rowButton);

                for (int j = 0; j < width; j++)
                {
                    Button tileButton = new Button
                    {
                        DataContext = Board.Tiles[j, i],
                        Style = tileStyle
                    };
                    Buttons.Add(Board.Tiles[j, i], tileButton);
                    widthStackPanel.Children.Add(tileButton);
                }

                HeightStackPanel.Children.Add(widthStackPanel);
            }

            Board.BoardChanging += Board_BoardChanging;
            Board.BoardChanged += Board_BoardChangedAsync;
            Board.GameOver += Board_GameOver;
        }

        private async void Board_BoardChangedAsync(BoardChangedEventArgs e)
        {
            if (e.Tile != null)
                Buttons[e.Tile].BorderBrush = new SolidColorBrush(Colors.Red);

            // AI
            if (!Board.IsGameOver && e.Player.IsAuto && UseAI.IsChecked == true)
                await RunAI();
        }

        private void Board_BoardChanging(BoardChangingEventArgs e)
        {
            if (e.Tile != null)
                Buttons[e.Tile].BorderBrush = new SolidColorBrush(Colors.Gray);
        }

        private void Board_GameOver(GameOverEventArgs e)
        {
            if (e.Winner == null)
                MessageBox.Show("Tie!");
            else
                MessageBox.Show(e.Winner.Name + " wins!");

            var players = Board.Players;
            Player last = players[players.Count - 1];
            players.RemoveAt(players.Count - 1);
            players.Insert(0, last);
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || Board.IsGameOver)
                return;

            Button button = sender as Button;
            if (button.DataContext != null)
            {
                Tile tile = button.DataContext as Tile;
                CleanAnalyze();
                Board.Play(tile);
            }
        }

        private async void UseAICheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // AI
            if (Board.GetCurrentPlayer().IsAuto && UseAI.IsChecked == true)
                await RunAI();
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            await AIPlayAsync();
            foreach (var tile in Choices)
            {
                Buttons[tile].BorderThickness = new Thickness(2.0);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            CleanAnalyze();
            Board.Restart();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            CleanAnalyze();
            Board.Undo();
        }

        private void DemoCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Board.Players.Find(p => p.Name.Contains("1")).IsAuto = true;
            UseAI.IsChecked = true;
        }

        private void DemoCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UseAI.IsChecked = false;
            Board.Players.Find(p => p.Name.Contains("1")).IsAuto = false;
        }
    }
}
