using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public MainWindow() :
      this(15, 15,
        new List<Player>()
        {
          new Player("Player 1", new Piece(Pieces.X), new GomokuAIv1(), false),
          new Player("Player 2", new Piece(Pieces.O), new GomokuAIAbpMinimax(), true),
        })
    {
    }

    public MainWindow(int boardWidth, int boardHeight, IList<Player> players)
    {
      InitializeComponent();

      Game = new Game(boardWidth, boardHeight, players);
      BoardVM = new BoardVM(Game);
      InitializeBoard(boardWidth, boardHeight);
      Game.BoardChanged += Board_BoardChangedAsync;
      Game.GameOver += Board_GameOver;
    }

    public BoardVM BoardVM { get; }
    public Game Game { get; }

    private async Task<IPositional> AIPlayAsync(bool showAnalysis = false)
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

      if (showAnalysis)
      {
        BoardVM.ClearHighlightedTiles();
        BoardVM.Highlight(result.PossibleChoices);
        BoardVM.Highlight(Game.LastMove);
      }

      BoardBorder.IsEnabled = true;

      return result.SelectedChoice;
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
      if (Game.IsOver)
      {
        return;
      }

      IPositional selectedTile = await AIPlayAsync(showAnalysis: true);
      BoardVM.Select(selectedTile);
    }

    private async void Board_BoardChangedAsync(object sender, BoardChangedEventArgs e)
    {
      // AI
      if (!Game.IsOver
          && Game.Manager.CurrentPlayer.IsAuto
          && UseAIToggleButton.IsChecked == true)
      {
        await RunAI();
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

      DemoToggleButton.IsChecked = false;
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
      // Gets horizontal stack style resource
      var widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;

      // Gets clickable tile style resource
      var tileStyle = Resources["TileButtonStyle"] as Style;

      // Gets non-clickable tile style resource
      var coorTileStlye = Resources["CoordinateTileButtonStyle"] as Style;

      // Creates first row (coordinates row that has alphabetic characters)
      var columnStackPanel = new StackPanel
      {
        Style = widthStackPanelStyle
      };

      // Add blank tile (top-left tile)
      var blankTile = new Button
      {
        Style = coorTileStlye,
        Content = ' ',
      };
      columnStackPanel.Children.Add(blankTile);

      // Add the rest of coordinate tiles with names
      for (var j = 0; j < width; j++)
      {
        var coordinateButton = new Button
        {
          Style = coorTileStlye,
#if DEBUG

          // If in debug mode, keep original index
          Content = j,
#else

          // Otherwise, use alphabetic characters
          Content = (char)(j + 'a'),
#endif
        };
        columnStackPanel.Children.Add(coordinateButton);
      }

      // Add first line to the vertical stack
      HeightStackPanel.Children.Add(columnStackPanel);

      // Add the rest of rows
      for (var i = 0; i < height; i++)
      {
        // Creates new horizontal stack representing a row
        var widthStackPanel = new StackPanel
        {
          Style = widthStackPanelStyle
        };

        // Add coordinate button (left-most column)
        var coordinateButton = new Button
        {
          Style = coorTileStlye,
#if DEBUG

          // If in debug mode, keep original index
          Content = i
#else

          // Otherwise, make one-based index
          Content = i + 1
#endif
        };

        // Add the coordinate button to the horizontal stack
        widthStackPanel.Children.Add(coordinateButton);

        // Add the rest of clickable buttons to the stack
        for (var j = 0; j < width; j++)
        {
          var tileButton = new Button
          {
            DataContext = BoardVM[j, i],
            Style = tileStyle
          };
          widthStackPanel.Children.Add(tileButton);
        }

        // Add the horizontal stack to the vertical stack
        HeightStackPanel.Children.Add(widthStackPanel);
      }
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
        Game.Play(tileVM.Tile.X, tileVM.Tile.Y);
      }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
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