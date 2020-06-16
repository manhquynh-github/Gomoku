using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Gomoku.AI;
using Gomoku.Logic;
using Gomoku.ViewModels;

namespace Gomoku
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
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

      Game = new Game(boardWidth, boardHeight, players);
      BoardVM = new BoardVM(Game.Board);
      Choices = new List<Tile>();
      InitializeBoard(boardWidth, boardHeight);
    }

    public BoardVM BoardVM { get; }
    public Game Game { get; }

    private async Task<Tile> AIPlayAsync()
    {
      Player player = Game.GetCurrentPlayer();

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
      Tuple<Tile, IEnumerable<Tile>> result = await player.AI.PlayAsync(Game);
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
      if (Game.IsOver)
      {
        return;
      }

      await AIPlayAsync();
      CleanAnalyze();
      foreach (Tile tile in Choices)
      {
        BoardVM[tile].IsHighlighted = true;
      }
    }

    private async void Board_BoardChangedAsync(BoardChangedEventArgs e)
    {
      if (e.Tile != null)
      {
        BoardVM.Set(e.Tile);
      }

      // AI
      if (!Game.IsOver && e.Player.IsAuto && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }

    private void Board_BoardChanging(BoardChangingEventArgs e)
    {
      if (e.Tile != null)
      {
        BoardVM[e.Tile].IsHighlighted = false;
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

      List<Player> players = Game.Players;
      Player last = players[players.Count - 1];
      players.RemoveAt(players.Count - 1);
      players.Insert(0, last);

      foreach (Tile tile in e.WinningLine.SameTiles)
      {
        BoardVM[tile].IsHighlighted = true;
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
          BoardVM[tile].IsHighlighted = false;
        }
      }
    }

    private async void DemoToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      if (Game.IsOver)
      {
        CleanAnalyze();
        Game.Restart();
      }

      foreach (Player player in Game.Players)
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
      Game.Players.First().IsAuto = false;
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
            DataContext = BoardVM[j, i],
            Style = tileStyle
          };
          widthStackPanel.Children.Add(tileButton);
        }

        HeightStackPanel.Children.Add(widthStackPanel);
      }

      Game.BoardChanging += Board_BoardChanging;
      Game.BoardChanged += Board_BoardChangedAsync;
      Game.GameOver += Board_GameOver;
    }

    private void MessageGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      ((Grid)sender).Visibility = Visibility.Collapsed;
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      foreach (var tile in Game.History)
      {
        BoardVM.Clear(tile);
      }

      Game.Restart();

      DemoToggleButton.IsChecked = false;
    }

    private async Task RunAI()
    {
      Tile tile = await AIPlayAsync();
      if (tile == null)
      {
        return;
      }

      CleanAnalyze();
      Game.Play(BoardVM[tile].Tile);
    }

    private void ShowMessage(string message)
    {
      MessageTextBlock.Text = message;
      MessageGrid.Visibility = Visibility.Visible;
    }

    private void TileButton_Click(object sender, RoutedEventArgs e)
    {
      if (sender == null || Game.IsOver)
      {
        return;
      }

      if (sender is Button button
        && button.DataContext != null)
      {
        var tileVM = button.DataContext as TileVM;
        CleanAnalyze();
        Game.Play(tileVM.Tile);
      }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      BoardVM.Clear(Game.LastPlayedTile);
      Game.Undo();
    }

    private async void UseAIToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      // AI
      if (DemoToggleButton.IsChecked == false
        && Game.GetCurrentPlayer().IsAuto
        && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }
  }
}