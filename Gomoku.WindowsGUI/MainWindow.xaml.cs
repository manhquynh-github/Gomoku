using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Gomoku.AI.Common.Algorithms;
using Gomoku.AI.Custom.Algorithms;
using Gomoku.Logic;
using Gomoku.Logic.AI;
using Gomoku.WindowsGUI.ViewModels;

namespace Gomoku.WindowsGUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly List<IPositional> _choices;

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
      _choices = new List<IPositional>();
      InitializeBoard(boardWidth, boardHeight);
    }

    public BoardVM BoardVM { get; }
    public Game Game { get; }

    private async Task<IPositional> AIPlayAsync()
    {
      Player player = Game.Manager.CurrentPlayer;

      if (player is null)
      {
        throw new InvalidOperationException($"{nameof(player)} is null.");
      }

      if (player.AI is null)
      {
        throw new InvalidOperationException($"{nameof(player.AI)} is null.");
      }

      BoardBorder.IsEnabled = false;

      var sw = Stopwatch.StartNew();
      AnalysisResult result = await Task.Run(() => player.AI.Analyze(Game));
      sw.Stop();
      if (sw.ElapsedMilliseconds < 500)
      {
        var delay = 500 - sw.ElapsedMilliseconds;
        await Task.Delay((int)delay);
      }

      _choices.Clear();
      _choices.AddRange(result.PossibleChoices);

      BoardBorder.IsEnabled = true;

      return result.SelectedChoice;
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
      if (Game.IsOver)
      {
        return;
      }

      IPositional selectedTile = await AIPlayAsync();
      CleanAnalyze();
      foreach (Tile tile in _choices)
      {
        BoardVM[tile.X, tile.Y].IsHighlighted = true;
      }
      BoardVM[selectedTile.X, selectedTile.Y].IsSelected = true;
    }

    private async void Board_BoardChangedAsync(object sender, BoardChangedEventArgs e)
    {
      Tile tile = e.Tile;

      if (!(tile is null))
      {
        BoardVM.Set(tile.X, tile.Y, tile.Piece);
      }

      // AI
      if (!Game.IsOver && e.Player.IsAuto && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }

    private void Board_BoardChanging(object sender, BoardChangingEventArgs e)
    {
      Tile tile = e.Tile;

      if (!(tile is null))
      {
        BoardVM[tile.X, tile.Y].IsHighlighted = false;
      }
    }

    private void Board_GameOver(object sender, GameOverEventArgs e)
    {
      if (e.Winner is null)
      {
        ShowMessage("Tie!");
      }
      else
      {
        ShowMessage($"{e.Winner.Name} wins!");
      }

      foreach (Tile tile in e.WinningTiles)
      {
        BoardVM[tile.X, tile.Y].IsHighlighted = true;
        _choices.Add(tile);
      }

      DemoToggleButton.IsChecked = false;
    }

    private void CleanAnalyze()
    {
      if (!(_choices is null) && _choices.Count > 0)
      {
        foreach (Tile tile in _choices)
        {
          BoardVM[tile.X, tile.Y].IsHighlighted = false;
        }
      }
    }

    private async void DemoToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      if (Game.IsOver)
      {
        RestartButton_Click(null, null);
      }

      foreach (Player player in Game.Manager.Players)
      {
        player.IsAuto = true;
      }

      UseAIToggleButton.IsChecked = true;
      UseAIToggleButton.IsEnabled = false;
      AnalyzeButton.IsEnabled = false;
      RestartButton.IsEnabled = false;
      UndoButton.IsEnabled = false;

      await RunAI();
    }

    private void DemoToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
      UseAIToggleButton.IsChecked = false;
      UseAIToggleButton.IsEnabled = true;
      AnalyzeButton.IsEnabled = true;
      RestartButton.IsEnabled = true;
      UndoButton.IsEnabled = true;
      Game.Manager.Players.First().IsAuto = false;
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
      if (MessageGrid.Visibility != Visibility.Collapsed)
      {
        MessageGrid.Visibility = Visibility.Collapsed;
      }
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      foreach (Tile tile in Game.History)
      {
        BoardVM.Clear(tile.X, tile.Y);
      }

      Game.Restart();
      MessageGrid_PreviewMouseDown(null, null);
      DemoToggleButton.IsChecked = false;
    }

    private async Task RunAI()
    {
      IPositional tile = await AIPlayAsync();
      if (tile is null)
      {
        return;
      }

      CleanAnalyze();
      Game.Play(tile.X, tile.Y);
    }

    private void ShowMessage(string message)
    {
      MessageTextBlock.Text = message;
      MessageGrid.Visibility = Visibility.Visible;
    }

    private void TileButton_Click(object sender, RoutedEventArgs e)
    {
      if (sender is null || Game.IsOver)
      {
        return;
      }

      if (sender is Button button
        && !(button.DataContext is null))
      {
        var tileVM = button.DataContext as TileVM;
        CleanAnalyze();
        Game.Play(tileVM.Tile.X, tileVM.Tile.Y);
      }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
      CleanAnalyze();
      if (Game.LastMove != null)
      {
        BoardVM.Clear(Game.LastMove.X, Game.LastMove.Y);
      }

      Game.Undo();
    }

    private async void UseAIToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      // AI
      if (!Game.IsOver
        && DemoToggleButton.IsChecked == false
        && Game.Manager.CurrentPlayer.IsAuto
        && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
      }
    }
  }
}