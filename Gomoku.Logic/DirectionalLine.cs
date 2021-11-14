using System;
using System.Collections.Generic;
using System.Linq;

namespace Gomoku.Logic
{
  /// <summary>
  /// Represents a directional group of <see cref="Tile"/> s that are adjacent
  /// to each other.
  /// </summary>
  public class DirectionalLine
  {
    /// <summary>
    /// Gets an instance of an empty <see cref="DirectionalLine"/>.
    /// </summary>
    public static readonly DirectionalLine EMPTY = new DirectionalLine();

    private DirectionalLine() : this(
      (Piece)Pieces.None,
      Directions.None,
      new Tile[0],
      new Tile[0],
      new Tile[0],
      new Tile[0],
      false)
    {
    }

    private DirectionalLine(
      Piece piece,
      Directions diretion,
      Tile[] tiles,
      Tile[] sameTiles,
      Tile[] blankTiles,
      Tile[] blockTiles,
      bool isChained)
    {
      Piece = piece;
      Direction = diretion;
      Tiles = tiles;
      SameTiles = sameTiles;
      BlankTiles = blankTiles;
      BlockTiles = blockTiles;
      IsChained = isChained;
    }

    /// <summary>
    /// All the tiles that have <see cref="Pieces.None"/>.
    /// </summary>
    public Tile[] BlankTiles { get; }

    /// <summary>
    /// All the tiles that have a different <see cref="Logic.Piece"/> other than <see cref="Piece"/>.
    /// </summary>
    public Tile[] BlockTiles { get; }

    /// <summary>
    /// The <see cref="Directions"/> that this <see cref="DirectionalLine"/> traverses.
    /// </summary>
    public Directions Direction { get; }

    /// <summary>
    /// Determines if all <see cref="SameTiles"/> are all next to each other
    /// without a <see cref="Pieces.None"/> tile in-between.
    /// </summary>
    public bool IsChained { get; }

    /// <summary>
    /// The original <see cref="Piece"/> of this <see cref="DirectionalLine"/>
    /// </summary>
    public Piece Piece { get; }

    /// <summary>
    /// All the tiles that have <see cref="Piece"/>.
    /// </summary>
    public Tile[] SameTiles { get; }

    /// <summary>
    /// All tiles
    /// </summary>
    public Tile[] Tiles { get; }

    /// <summary>
    /// Creates an <see cref="DirectionalLine"/> by traversing the
    /// <paramref name="board"/> starting at position <paramref name="x"/>,
    /// <paramref name="y"/> towards two symmetrical <see cref="Directions"/>
    /// (depending on the <paramref name="orientation"/>) where
    /// <paramref name="piece"/> will determine the <see cref="Tile"/> this line
    /// is for. The iteration will stop if (1) <paramref name="maxTile"/> is
    /// reached, or (2) a <see cref="Logic.Piece"/> other than
    /// <paramref name="piece"/> is encountered, or (3) the
    /// <paramref name="blankTolerance"/> number of <see cref="Pieces.None"/>
    /// tiles are encountered. The starting position itself is not included in
    /// the traversal.
    /// </summary>
    /// <param name="board">the <see cref="Board"/> to search</param>
    /// <param name="x">the starting <see cref="Tile.X"/></param>
    /// <param name="y">the starting <see cref="Tile.Y"/></param>
    /// <param name="piece">
    /// the <see cref="Logic.Piece"/> that will determine <see cref="GetSameTiles()"/>
    /// </param>
    /// <param name="orientation">the <see cref="Orientations"/> to traverse</param>
    /// <param name="maxTile">the max number of <see cref="Tile"/> to traverse</param>
    /// <param name="blankTolerance">
    /// the max number of <see cref="Pieces.None"/> tiles before the traversing stops
    /// </param>
    /// <returns>a <see cref="DirectionalLine"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static DirectionalLine FromBoard(
      Board board,
      int x,
      int y,
      Piece piece,
      Directions direction,
      int maxTile,
      int blankTolerance)
    {
      if (board is null)
      {
        throw new ArgumentNullException(nameof(board));
      }

      if (x < 0 || x > board.Width)
      {
        throw new ArgumentOutOfRangeException(nameof(x));
      }

      if (y < 0 || y > board.Height)
      {
        throw new ArgumentOutOfRangeException(nameof(y));
      }

      if (maxTile < 0)
      {
        throw new ArgumentException(nameof(maxTile));
      }

      if (blankTolerance < 0)
      {
        throw new ArgumentException(nameof(blankTolerance));
      }

      var tiles = new Queue<Tile>();
      var sameTiles = new Queue<Tile>();
      var blankTiles = new Queue<Tile>();
      var blockTiles = new Queue<Tile>();

      var count = 0;
      var chainBreak = false;
      var blank = 0;

      board.IterateTiles(
        x,
        y,
        direction,
        t =>
        {
          if (count++ == maxTile)
          {
            return false;
          }

          if (t.Piece.Type == piece)
          {
            if (blank > 0)
            {
              chainBreak = true;
            }

            tiles.Enqueue(t);
            sameTiles.Enqueue(t);
            return true;
          }
          else if (t.Piece.Type == Pieces.None)
          {
            if (blank++ == blankTolerance)
            {
              return false;
            }

            tiles.Enqueue(t);
            blankTiles.Enqueue(t);
            return true;
          }
          else
          {
            tiles.Enqueue(t);
            blockTiles.Enqueue(t);
            return false;
          }
        });

      return new DirectionalLine(
        piece,
        direction,
        tiles.ToArray(),
        sameTiles.ToArray(),
        blankTiles.ToArray(),
        blockTiles.ToArray(),
        !chainBreak);
    }

    public string ToString(bool includesSelf, bool followsLTRB)
    {
      IEnumerable<Tile> source = Tiles;

      if (followsLTRB)
      {
        switch (Direction)
        {
          case Directions.Left:
          case Directions.Up:
          case Directions.UpLeft:
          case Directions.DownLeft:
            source = Tiles.Reverse();
            break;

          case Directions.Right:
          case Directions.Down:
          case Directions.UpRight:
          case Directions.DownRight:
            break;

          default:
            throw new InvalidOperationException("Unexpected value.");
        }
      }

      var tilesString = string.Join(
        ',',
        source.Select(t => t.Piece.ToString()));

      return includesSelf
        ? string.Join(
          ',',
          Piece.ToString(),
          tilesString)
        : tilesString;
    }

    public override string ToString()
    {
      return ToString(includesSelf: true, followsLTRB: false);
    }
  }
}