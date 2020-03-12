using Gomoku.AI;
using Gomoku.BoardNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gomoku
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public readonly Board Board;
    public readonly Dictionary<Tile, Button> Buttons;
    private List<Tile> Choices;

    public MainWindow() :
      this(15, 15,
        new List<Player>()
        {
          new Player("Player 1", new Piece(Pieces.X), new GomokuAIv1(), false),
          new Player("Player 2", new Piece(Pieces.O), new GomokuAIv1(), true),
        })
    {
    }

    public MainWindow(int boardWidth, int boardHeight, IList<Player> players)
    {
      InitializeComponent();

      Board = new Board(boardWidth, boardHeight, players);
      Buttons = new Dictionary<Tile, Button>();
      InitializeBoard(boardWidth, boardHeight);
      Choices = new List<Tile>();
    }

    private async Task<Tile> AIPlayAsync()
    {
      Player player = Board.GetCurrentPlayer();

      if (player == null)
      {
        throw new InvalidOperationException($"{nameof(player)} is null.");
      }

      if (player.AI == null)
      {
        throw new InvalidOperationException($"{nameof(player.AI)} is null.");
      }

      BoardBorder.IsEnabled = false;

      var sw = Stopwatch.StartNew();
      Tuple<Tile, IEnumerable<Tile>> result = await player.AI.PlayAsync(Board);
      sw.Stop();
      if (sw.ElapsedMilliseconds < 500)
      {
        var delay = 500 - sw.ElapsedMilliseconds;
        await Task.Delay((int)delay);
      }

      Choices = result.Item2.ToList();

      BoardBorder.IsEnabled = true;

      return result.Item1;
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
      if (Board.IsGameOver)
      {
        return;
      }

      await AIPlayAsync();
      CleanAnalyze();
      foreach (Tile tile in Choices)
      {
        tile.IsHighlighted = true;
      }
    }

    private async void Board_BoardChangedAsync(BoardChangedEventArgs e)
    {
      if (e.Tile != null)
      {
        e.Tile.IsHighlighted = true;
      }

      // AI
      if (!Board.IsGameOver && e.Player.IsAuto && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }

    private void Board_BoardChanging(BoardChangingEventArgs e)
    {
      if (e.Tile != null)
      {
        e.Tile.IsHighlighted = false;
      }
    }

    private void Board_GameOver(GameOverEventArgs e)
    {
      if (e.Winner == null)
      {
        ShowMessage("Tie!");
      }
      else
      {
        ShowMessage($"{e.Winner.Name} wins!");
      }

      List<Player> players = Board.Players;
      Player last = players[players.Count - 1];
      players.RemoveAt(players.Count - 1);
      players.Insert(0, last);

      foreach (Tile tile in e.WinningLine.SameTiles)
      {
        tile.IsHighlighted = true;
        Choices.Add(tile);
      }

      DemoToggleButton.IsChecked = false;
    }

    private void CleanAnalyze()
    {
      if (Choices != null && Choices.Count() > 0)
      {
        foreach (Tile tile in Choices)
        {
          tile.IsHighlighted = false;
        }
      }
    }

    private async void DemoToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      if (Board.IsGameOver)
      {
        CleanAnalyze();
        Board.Restart();
      }

      foreach (Player player in Board.Players)
      {
        player.IsAuto = true;
      }

      UseAIToggleButton.IsChecked = true;
      UseAIToggleButton.IsEnabled = false;
      CleanAnalyze();

      await RunAI();
    }

    private void DemoToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
      UseAIToggleButton.IsChecked = false;
      UseAIToggleButton.IsEnabled = true;
      Board.Players.First().IsAuto = false;
    }

    private void InitializeBoard(int width, int height)
    {
      var widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;
      var tileStyle = Resources["TileButtonStyle"] as Style;
      var coorTileStlye = Resources["CoordinateTileButtonStyle"] as Style;

      var columnStackPanel = new StackPanel
      {
        Style = widthStackPanelStyle
      };
      for (var j = -1; j < width; j++)
      {
        var tileButton = new Button
        {
          Style = coorTileStlye,
          Content = j == -1
            ? ' '
            : (char)(j + char.Parse("a")),
        };
        columnStackPanel.Children.Add(tileButton);
      }
      HeightStackPanel.Children.Add(columnStackPanel);

      for (var i = 0; i < height; i++)
      {
        var widthStackPanel = new StackPanel
        {
          Style = widthStackPanelStyle
        };

        var rowButton = new Button
        {
          Style = coorTileStlye,
          Content = i + 1
        };
        widthStackPanel.Children.Add(rowButton);

        for (var j = 0; j < width; j++)
        {
          var tileButton = new Button
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

    private void MessageGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      ((Grid)sender).Visibility = Visibility.Collapsed;
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      Board.Restart();
      DemoToggleButton.IsChecked = false;
    }

    private async Task RunAI()
    {
      Tile tile = await AIPlayAsync();
      if (tile == null)
      {
        return;
      }

      TileButton_Click(Buttons[tile], null);
    }

    private void ShowMessage(string message)
    {
      MessageTextBlock.Text = message;
      MessageGrid.Visibility = Visibility.Visible;
    }

    private void TileButton_Click(object sender, RoutedEventArgs e)
    {
      if (sender == null || Board.IsGameOver)
      {
        return;
      }

      var button = sender as Button;
      if (button.DataContext != null)
      {
        var tile = button.DataContext as Tile;
        CleanAnalyze();
        Board.Play(tile);
      }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      Board.Undo();
    }

    private async void UseAIToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      // AI
      if (DemoToggleButton.IsChecked == false
        && Board.GetCurrentPlayer().IsAuto
        && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }
  }
}