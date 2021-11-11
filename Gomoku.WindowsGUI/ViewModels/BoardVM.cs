using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Gomoku.Logic;

namespace Gomoku.WindowsGUI.ViewModels
{
  public class BoardVM
  {
    public BoardVM(Game game)
    {
      Board = game.Board;

      TileVMs = new TileVM[Board.Width, Board.Height];
      for (var i = 0; i < Board.Width; i++)
      {
        for (var j = 0; j < Board.Height; j++)
        {
          TileVMs[i, j] = new TileVM(Board[i, j]);
        }
      }

      HighlightedTiles = new ObservableCollection<TileVM>();
      HighlightedTiles.CollectionChanged += HighlightedTiles_CollectionChanged;
      game.BoardChanged += Game_BoardChanged;
      game.GameOver += Game_GameOver;
    }

    public Board Board { get; }

    private ObservableCollection<TileVM> HighlightedTiles { get; }

    private TileVM[,] TileVMs { get; }

    public TileVM this[int x, int y] => TileVMs[x, y];

    public void Clear(int x, int y)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      TileVM tileVM = this[x, y];
      tileVM.Piece = (Piece)Pieces.None;
      HighlightedTiles.Remove(tileVM);
    }

    public void ClearHighlightedTiles()
    {
      int count;
      while ((count = HighlightedTiles.Count) > 0)
      {
        HighlightedTiles.RemoveAt(count - 1);
      }
    }

    public void Highlight(IEnumerable<IPositional> positionals)
    {
      foreach (IPositional position in positionals)
      {
        HighlightedTiles.Add(this[position.X, position.Y]);
      }
    }

    public void Highlight(params IPositional[] positionals)
    {
      foreach (IPositional position in positionals)
      {
        HighlightedTiles.Add(this[position.X, position.Y]);
      }
    }

    public void Select(IPositional position)
    {
      this[position.X, position.Y].IsSelected = true;
    }

    public void Set(int x, int y, Piece piece)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      TileVM tileVM = this[x, y];
      tileVM.Piece = piece;
      HighlightedTiles.Add(tileVM);
    }

    private void Game_BoardChanged(object sender, BoardChangedEventArgs e)
    {
      ClearHighlightedTiles();

      if (e.RemovedTiles.Count > 0)
      {
        foreach (Tile tile in e.RemovedTiles)
        {
          Clear(tile.X, tile.Y);
        }

        Tile lastTile = ((Game)sender).LastMove;
        if (lastTile != null)
        {
          Highlight(lastTile);
        }
      }

      foreach (Tile tile in e.AddedTiles)
      {
        Set(tile.X, tile.Y, tile.Piece);
      }
    }

    private void Game_GameOver(object sender, GameOverEventArgs e)
    {
      foreach (Tile tile in e.WinningTiles)
      {
        HighlightedTiles.Add(this[tile.X, tile.Y]);
      }
    }

    private void HighlightedTiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Remove
        || e.Action == NotifyCollectionChangedAction.Reset)
      {
        foreach (TileVM item in e.OldItems)
        {
          item.IsHighlighted = false;
        }
      }

      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (TileVM item in e.NewItems)
        {
          item.IsHighlighted = true;
        }
      }
    }
  }
}