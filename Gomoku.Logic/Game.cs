﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a Gomoku game
  /// </summary>
  public class Game : ICloneable
  {
    public static readonly int WINPIECES = 5;

    private readonly List<Player> _players;

    public Game(int width, int height, IEnumerable<Player> players)
    {
      if (height <= WINPIECES || width <= WINPIECES)
      {
        throw new ArgumentException(
          $"{nameof(Game)} must have at least {WINPIECES}x{WINPIECES} board.");
      }

      Board = new Board(width, height);
      MaxMove = width * height;
      _players = new List<Player>(players);
      Turn = 0;
      History = new Stack<Tile>();
      IsOver = false;
      ShiftPlayersOnGameOver = true;
    }

    public Game(Game g)
    {
      if (g is null)
      {
        throw new ArgumentNullException(nameof(g));
      }

      Board = g.Board.DeepClone();
      MaxMove = g.MaxMove;
      _players = new List<Player>(g._players);
      Turn = g.Turn;
      History = new Stack<Tile>(g.History);
      IsOver = g.IsOver;
      ShiftPlayersOnGameOver = g.ShiftPlayersOnGameOver;
    }

    public event BoardChangedEventHandler BoardChanged;

    public event BoardChangingEventHandler BoardChanging;

    public event GameOverEventHandler GameOver;

    public Board Board { get; }
    public Player CurrentPlayer => _players[Turn];
    public Stack<Tile> History { get; set; }
    public bool IsOver { get; private set; }
    public bool IsTie => History.Count == MaxMove;
    public Tile LastPlayedTile => History.Count == 0 ? null : History.Peek();
    public int MaxMove { get; }
    public IReadOnlyList<Player> Players => _players;
    public bool ShiftPlayersOnGameOver { get; set; }
    public int Turn { get; set; }
    public int Height => Board.Height;
    public int Width => Board.Width;

    public bool CheckGameOver(int x, int y, out IEnumerable<Tile> winningTiles)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      Tile tile = Board[x, y];

      if (tile.Piece.Type != Pieces.None)
      {
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          var line = OrientedlLine.FromBoard(
            Board,
            tile.X,
            tile.Y,
            tile.Piece,
            orientation,
            maxTile: WINPIECES,
            blankTolerance: 0);

          if (line.IsChained
            && line.SameTileCount + 1 == WINPIECES
            && line.BlockTilesCount < 2)
          {
            var result = line.Tiles.ToList();
            result.Add(tile);
            winningTiles = result;
            return true;
          }
        }
      }

      winningTiles = new List<Tile>();
      return false;
    }

    public object Clone()
    {
      return MemberwiseClone();
    }

    public Game DeepClone()
    {
      return new Game(this);
    }

    public int GetPlayersTurn(Player player)
    {
      if (player == null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return _players.FindIndex(p => p == player);
    }

    public bool IsPlayersTurn(Player player)
    {
      if (player == null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return CurrentPlayer == player;
    }

    public void Play(int x, int y)
    {
      // Check if game is over
      if (IsOver)
      {
        return;
      }

      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      Tile tile = Board[x, y];

      // Check for already placed tile
      if (tile.Piece.Type != Pieces.None)
      {
        return;
      }

      Player oldPlayer = CurrentPlayer;
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, oldPlayer, LastPlayedTile));

      tile.Piece = oldPlayer.Piece;
      History.Push(tile);

      // Check for game over
      if (IsTie)
      {
        IsOver = true;
        GameOver?.Invoke(new GameOverEventArgs(true, Turn, null, new List<Tile>()));

        if (ShiftPlayersOnGameOver)
        {
          ShiftPlayers();
        }
      }
      else if (CheckGameOver(x, y, out IEnumerable<Tile> winningLine))
      {
        IsOver = true;
        GameOver?.Invoke(new GameOverEventArgs(true, Turn, oldPlayer, winningLine));

        if (ShiftPlayersOnGameOver)
        {
          ShiftPlayers();
        }
      }
      else
      {
        // Increment turn
        Turn = (Turn + 1) % _players.Count;
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, tile));
    }

    public void Restart()
    {
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, CurrentPlayer, LastPlayedTile));

      foreach (Tile tile in History)
      {
        tile.Piece = new Piece(Pieces.None);
      }

      Turn = 0;
      History.Clear();
      IsOver = false;

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, null));
    }

    public void Undo()
    {
      if (History.Count == 0)
      {
        return;
      }

      Tile tile = History.Pop();
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, CurrentPlayer, tile));

      tile.Piece = new Piece(Pieces.None);
      Turn = (Turn - 1 + _players.Count) % _players.Count;
      if (IsOver)
      {
        IsOver = false;
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, LastPlayedTile));
    }

    private void ShiftPlayers()
    {
      if (_players is null
        || !_players.Any())
      {
        throw new InvalidOperationException("Player list is null or empty");
      }

      Player last = _players[^1];
      _players.RemoveAt(_players.Count - 1);
      _players.Insert(0, last);
    }
  }
}