using System;
using System.Collections.Generic;
using System.Linq;

namespace Gomoku.BoardNS
{
  public class Board : ICloneable
  {
    public static readonly int WINPIECES = 5;

    public Board(int width, int height, IList<Player> players)
    {
      if (height <= 5 || width <= 5)
      {
        throw new ArgumentException();
      }

      Width = width;
      Height = height;
      Tiles = new Tile[Width, Height];
      for (var i = 0; i < Width; i++)
      {
        for (var j = 0; j < Height; j++)
        {
          Tiles[i, j] = new Tile(i, j);
        }
      }

      MaxMove = Width * Height;
      Players = new List<Player>(players);
      Turn = 0;
      History = new Stack<Tile>();
      IsGameOver = false;
    }

    public event BoardChangedEventHandler BoardChanged;
    public event BoardChangingEventHandler BoardChanging;
    public event GameOverEventHandler GameOver;

    public int Height { get; }
    public Stack<Tile> History { get; set; }
    public bool IsGameOver { get; private set; }
    public bool IsTie => History.Count == MaxMove;
    public Tile LastPlayedTile => History.Count == 0 ? null : History.Peek();
    public int MaxMove { get; }
    public List<Player> Players { get; }
    public Tile[,] Tiles { get; }
    public int Turn { get; set; }
    public int Width { get; }

    public object Clone()
    {
      return MemberwiseClone();
    }

    public object DeepCopy()
    {
      var b = new Board(Width, Height, Players);
      for (var i = 0; i < Width; i++)
      {
        for (var j = 0; j < Height; j++)
        {
          Tile tile = Tiles[i, j];
          b.Tiles[i, j] = new Tile(i, j)
          {
            Piece = tile.Piece
          };
        }
      }

      b.Turn = Turn;
      b.History = new Stack<Tile>(History);
      b.IsGameOver = IsGameOver;
      return b;

      // return this.MemberwiseClone();
    }

    public Player GetCurrentPlayer()
    {
      return Players[Turn];
    }

    public Line GetLine(
        Tile tile,
        Direction direction,
        Pieces type,
        int max = 5)
    {
      var sameTiles = new List<Tile>();
      var blankTiles = new List<Tile>();
      var blockTiles = new List<Tile>();

      var count = 0;
      var chainBreak = false;
      var previouslyBlank = false;

      RunTilesFunction(
          tile,
          direction,
          t =>
          {
            if (count++ == max)
            {
              return false;
            }

            if (t.Piece.Type == type)
            {
              if (previouslyBlank)
              {
                chainBreak = true;
              }

              previouslyBlank = false;

              sameTiles.Add(t);
              return true;
            }
            else if (t.Piece.Type == Pieces.None)
            {
              if (previouslyBlank || chainBreak)
              {
                return false;
              }

              previouslyBlank = true;

              blankTiles.Add(t);
              return true;
            }
            else
            {
              if (previouslyBlank)
              {
                return false;
              }

              blockTiles.Add(t);
              return false;
            }
          });

      return new Line(sameTiles, blankTiles, blockTiles, !chainBreak);
    }

    public LineGroup GetLineGroup(
        Tile tile,
        Orientation orientation,
        Pieces type,
        int max = 5)
    {
      switch (orientation)
      {
        case Orientation.Horizontal:
          return new LineGroup(
              GetLine(tile, Direction.Left, type, max),
              GetLine(tile, Direction.Right, type, max));

        case Orientation.Vertical:
          return new LineGroup(
              GetLine(tile, Direction.Up, type, max),
              GetLine(tile, Direction.Down, type, max));

        case Orientation.Diagonal:
          return new LineGroup(
              GetLine(tile, Direction.UpLeft, type, max),
              GetLine(tile, Direction.DownRight, type, max));

        case Orientation.RvDiagonal:
          return new LineGroup(
              GetLine(tile, Direction.UpRight, type, max),
              GetLine(tile, Direction.DownLeft, type, max));
      }

      return new LineGroup();
    }

    public void Play(Tile tile)
    {
      // Check if game is over
      if (IsGameOver)
      {
        return;
      }

      Tile _tile = Tiles[tile.X, tile.Y];

      // Check for already placed tile
      if (_tile.Piece.Type != Pieces.None)
      {
        return;
      }

      Player oldPlayer = GetCurrentPlayer();
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, oldPlayer, LastPlayedTile));

      _tile.Piece = oldPlayer.Piece;
      History.Push(_tile);

      // Check for game over
      if (IsTie)
      {
        IsGameOver = true;
        GameOver?.Invoke(new GameOverEventArgs(true, Turn, null, Line.EMPTY));
      }
      else
      {
        for (var i = 0; i <= 3; i++)
        {
          LineGroup lineGroup = GetLineGroup(_tile, (Orientation)i, _tile.Piece.Type);

          if (lineGroup.IsChained
              && lineGroup.SameTileCount + 1 == WINPIECES
              && lineGroup.BlockTileCount < 2)
          {
            IsGameOver = true;

            var winningTiles =
              (from line in lineGroup.Lines
               from t in line.SameTiles
               select t)
              .ToList();

            winningTiles.Add(tile);
            var winningLine = new Line(winningTiles);

            GameOver?.Invoke(new GameOverEventArgs(true, Turn, oldPlayer, winningLine));
            break;
          }
        }

        if (!IsGameOver)
        {
          // Increment turn
          Turn = (Turn + 1) % Players.Count;
        }
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), _tile));
    }

    public void Restart()
    {
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, GetCurrentPlayer(), LastPlayedTile));

      foreach (Tile tile in History)
      {
        tile.Piece = new Piece(Pieces.None);
        tile.IsHighlighted = false;
      }

      Turn = 0;
      History.Clear();
      IsGameOver = false;

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), null));
    }

    public void RunTilesFunction(
                                Tile tile,
        Direction direction,
        Predicate<Tile> predicate)
    {
      switch (direction)
      {
        case Direction.Left:
          for (int i = tile.X - 1, j = tile.Y;
              i >= 0 && predicate(Tiles[i, j]);
              i--)
          {
            ;
          }

          break;

        case Direction.Right:
          for (int i = tile.X + 1, j = tile.Y;
              i < Width && predicate(Tiles[i, j]);
              i++)
          {
            ;
          }

          break;

        case Direction.Up:
          for (int i = tile.X, j = tile.Y - 1;
              j >= 0 && predicate(Tiles[i, j]);
              j--)
          {
            ;
          }

          break;

        case Direction.Down:
          for (int i = tile.X, j = tile.Y + 1;
              j < Height && predicate(Tiles[i, j]);
              j++)
          {
            ;
          }

          break;

        case Direction.UpLeft:
          for (int i = tile.X - 1, j = tile.Y - 1;
              i >= 0 && j >= 0 && predicate(Tiles[i, j]);
              i--, j--)
          {
            ;
          }

          break;

        case Direction.DownRight:
          for (int i = tile.X + 1, j = tile.Y + 1;
              i < Width && j < Height && predicate(Tiles[i, j]);
              i++, j++)
          {
            ;
          }

          break;

        case Direction.UpRight:
          for (int i = tile.X + 1, j = tile.Y - 1;
              i < Width && j >= 0 && predicate(Tiles[i, j]);
              i++, j--)
          {
            ;
          }

          break;

        case Direction.DownLeft:
          for (int i = tile.X - 1, j = tile.Y + 1;
              i >= 0 && j < Height && predicate(Tiles[i, j]);
              i--, j++)
          {
            ;
          }

          break;
      }
    }

    public void Undo()
    {
      if (History.Count == 0)
      {
        return;
      }

      Tile tile = History.Pop();
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, GetCurrentPlayer(), tile));

      tile.Piece = new Piece(Pieces.None);
      Turn = (Turn - 1 + Players.Count) % Players.Count;
      if (IsGameOver)
      {
        IsGameOver = false;
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), LastPlayedTile));
    }
  }
}