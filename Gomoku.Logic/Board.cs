using System;
using System.Collections.Generic;

namespace Gomoku.Logic
{
  public class Board : ICloneable
  {
    public Board(int width, int height)
    {
      if (height < 0 || width < 0)
      {
        throw new ArgumentException($"{nameof(Board)} must have non-negative dimensions.");
      }

      Width = width;
      Height = height;
      Tiles = new Tile[Width, Height];
      for (int i = 0; i < Width; i++)
      {
        for (int j = 0; j < Height; j++)
        {
          Tiles[i, j] = new Tile(i, j)
          {
            Piece = (Piece)Pieces.None,
          };
        }
      }
    }

    public Board(Board b)
    {
      Width = b.Width;
      Height = b.Height;
      Tiles = new Tile[Width, Height];
      for (int i = 0; i < Width; i++)
      {
        for (int j = 0; j < Height; j++)
        {
          Tiles[i, j] = new Tile(i, j)
          {
            Piece = b[i, j].Piece,
          };
        }
      }
    }

    public int Height { get; }

    public int Width { get; }

    private Tile[,] Tiles { get; }

    public Tile this[int x, int y] => Tiles[x, y];

    public static Board Parse(string s)
    {
      if (string.IsNullOrWhiteSpace(s))
      {
        throw new FormatException($"{nameof(s)} must not be empty.");
      }

      string[] heightSplit = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      if (heightSplit.Length == 0)
      {
        throw new FormatException($"{nameof(s)} is not format compliant. (misformatted rows)");
      }

      string[] widthSplit = heightSplit[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
      if (widthSplit.Length == 0)
      {
        throw new FormatException($"{nameof(s)} is not format compliant. (misformatted columns)");
      }

      Board b = new Board(widthSplit.Length, heightSplit.Length);

      for (int j = 0; j < heightSplit.Length; j++)
      {
        widthSplit = heightSplit[j].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (widthSplit.Length != b.Width)
        {
          throw new FormatException($"{nameof(s)} is not format compliant. (jagged array)");
        }

        for (int i = 0; i < widthSplit.Length; i++)
        {
          string pieceStr = widthSplit[i];
          int piece;

          try
          {
            piece = int.Parse(pieceStr);
          }
          catch (Exception ex)
          {
            throw new FormatException($"{nameof(s)} is not format compliant. (unable to parse piece)", ex);
          }

          b[piece, j].Piece = (Piece)piece;
        }
      }

      return b;
    }

    public object Clone()
    {
      return MemberwiseClone();
    }

    public Board DeepClone()
    {
      return new Board(this);
    }

    public Line GetLine(
      Tile tile,
      Direction direction,
      Pieces type,
      int max)
    {
      List<Tile> sameTiles = new List<Tile>();
      List<Tile> blankTiles = new List<Tile>();
      List<Tile> blockTiles = new List<Tile>();

      int count = 0;
      bool chainBreak = false;
      bool previouslyBlank = false;

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
      int max)
    {
      return orientation switch
      {
        Orientation.Horizontal => new LineGroup(
          GetLine(tile, Direction.Left, type, max),
          GetLine(tile, Direction.Right, type, max)),
        Orientation.Vertical => new LineGroup(
          GetLine(tile, Direction.Up, type, max),
          GetLine(tile, Direction.Down, type, max)),
        Orientation.Diagonal => new LineGroup(
          GetLine(tile, Direction.UpLeft, type, max),
          GetLine(tile, Direction.DownRight, type, max)),
        Orientation.RvDiagonal => new LineGroup(
          GetLine(tile, Direction.UpRight, type, max),
          GetLine(tile, Direction.DownLeft, type, max)),
        _ => throw new InvalidOperationException("Unexpected value"),
      };
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
          }

          break;

        case Direction.Right:
          for (int i = tile.X + 1, j = tile.Y;
              i < Width && predicate(Tiles[i, j]);
              i++)
          {
          }

          break;

        case Direction.Up:
          for (int i = tile.X, j = tile.Y - 1;
              j >= 0 && predicate(Tiles[i, j]);
              j--)
          {
          }

          break;

        case Direction.Down:
          for (int i = tile.X, j = tile.Y + 1;
              j < Height && predicate(Tiles[i, j]);
              j++)
          {
          }

          break;

        case Direction.UpLeft:
          for (int i = tile.X - 1, j = tile.Y - 1;
              i >= 0 && j >= 0 && predicate(Tiles[i, j]);
              i--, j--)
          {
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
          }

          break;

        case Direction.DownLeft:
          for (int i = tile.X - 1, j = tile.Y + 1;
              i >= 0 && j < Height && predicate(Tiles[i, j]);
              i--, j++)
          {
          }

          break;
      }
    }

    public override string ToString()
    {
      string res = string.Empty;

      for (int j = 0; j < Height; j++)
      {
        for (int i = 0; i < Width; i++)
        {
          res += Tiles[i, j].Piece.TypeIndex.ToString() + ",";
        }

        res = res[0..^1];
        res += ";";
      }
      res = res[0..^1];

      return res;
    }
  }
}