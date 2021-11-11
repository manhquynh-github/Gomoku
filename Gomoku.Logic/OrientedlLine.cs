using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines group of <see cref="Tile"/> that is collected by traversing from
  /// one <see cref="Tile"/> on a <see cref="Board"/> towards two
  /// <see cref="Directions"/> respective to an <see cref="Orientations"/> until
  /// they reaches two <see cref="Tile"/> of <see cref="Pieces.None"/> or a
  /// <see cref="Tile"/> with a different <see cref="Piece"/>.
  /// </summary>
  public class OrientedlLine
  {
    /// <summary>
    /// Gets an instance of an empty <see cref="OrientedlLine"/>.
    /// </summary>
    public static readonly OrientedlLine EMPTY = new OrientedlLine();

    private OrientedlLine() : this(
      (Piece)Pieces.None,
      Orientations.None,
      DirectionalLine.EMPTY,
      DirectionalLine.EMPTY)
    {
    }

    private OrientedlLine(
      Piece piece,
      Orientations orientation,
      DirectionalLine firstLine,
      DirectionalLine secondLine)
    {
      Piece = piece;
      Orientation = orientation;
      FirstLine = firstLine;
      SecondLine = secondLine;

      Lines = ImmutableArray.Create(firstLine, secondLine);
      BlankTilesCount = firstLine.BlankTiles.Length + secondLine.BlankTiles.Length;
      BlockTilesCount = firstLine.BlockTiles.Length + secondLine.BlockTiles.Length;
      SameTileCount = firstLine.SameTiles.Length + secondLine.SameTiles.Length;
      TilesCount = firstLine.Tiles.Length + secondLine.Tiles.Length;
      IsChained = firstLine.IsChained && secondLine.IsChained;
    }

    /// <summary>
    /// Gets total number of the two <see cref="DirectionalLine.BlankTiles"/>.
    /// </summary>
    public int BlankTilesCount { get; }

    /// <summary>
    /// Gets total number of the two <see cref="DirectionalLine.BlockTiles"/>.
    /// </summary>
    public int BlockTilesCount { get; }

    /// <summary>
    /// The first <see cref="DirectionalLine"/> where <see cref="Directions"/>
    /// is either <see cref="Directions.Left"/>, <see cref="Directions.Up"/>,
    /// <see cref="Directions.UpLeft"/>, or <see cref="Directions.UpRight"/>.
    /// </summary>
    public DirectionalLine FirstLine { get; }

    /// <summary>
    /// Determines whether all <see cref="Lines"/> are <see cref="DirectionalLine.IsChained"/>.
    /// </summary>
    public bool IsChained { get; }

    /// <summary>
    /// Contains <see cref="FirstLine"/> and <see cref="SecondLine"/>.
    /// </summary>
    public ImmutableArray<DirectionalLine> Lines { get; }

    /// <summary>
    /// The <see cref="Orientations"/> that this <see cref="OrientedlLine"/> traverses.
    /// </summary>
    public Orientations Orientation { get; }

    /// <summary>
    /// The original <see cref="Piece"/> of this <see cref="OrientedlLine"/>
    /// </summary>
    public Piece Piece { get; }

    /// <summary>
    /// Gets total number of the two <see cref="DirectionalLine.SameTiles"/>.
    /// </summary>
    public int SameTileCount { get; }

    /// <summary>
    /// The first <see cref="DirectionalLine"/> where <see cref="Directions"/>
    /// is either <see cref="Directions.Right"/>, <see cref="Directions.Down"/>,
    /// <see cref="Directions.DownRight"/>, or <see cref="Directions.DownLeft"/>.
    /// </summary>
    public DirectionalLine SecondLine { get; }

    /// <summary>
    /// Gets total number of the two <see cref="DirectionalLine.Tiles"/>.
    /// </summary>
    public int TilesCount { get; }

    /// <summary>
    /// Creates a new <see cref="OrientedlLine"/> by traversing the
    /// <paramref name="board"/> starting at position <paramref name="x"/>,
    /// <paramref name="y"/> towards two symmetrical <see cref="Directions"/>
    /// depending on the <paramref name="orientation"/> where
    /// <paramref name="piece"/> will determine <see cref="GetSameTiles()"/>
    /// until <paramref name="maxTile"/> is reached, or a different
    /// <see cref="Logic.Piece"/> than <paramref name="piece"/> is encountered,
    /// or more than <paramref name="blankTolerance"/> number of
    /// <see cref="Pieces.None"/> tiles are encountered.
    /// </summary>
    /// <param name="board">the <see cref="Board"/> to be used</param>
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
    /// <returns>a <see cref="OrientedlLine"/></returns>
    public static OrientedlLine FromBoard(
      Board board,
      int x,
      int y,
      Piece type,
      Orientations orientation,
      int maxTile,
      int blankTolerance)
    {
      if (board is null)
      {
        throw new ArgumentNullException(nameof(board));
      }

      if (x < 0 || x > board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      return orientation switch
      {
        Orientations.Horizontal => new OrientedlLine(
          type,
          orientation,
          DirectionalLine.FromBoard(board, x, y, type, Directions.Left, maxTile, blankTolerance),
          DirectionalLine.FromBoard(board, x, y, type, Directions.Right, maxTile, blankTolerance)),
        Orientations.Vertical => new OrientedlLine(
          type,
          orientation,
          DirectionalLine.FromBoard(board, x, y, type, Directions.Up, maxTile, blankTolerance),
          DirectionalLine.FromBoard(board, x, y, type, Directions.Down, maxTile, blankTolerance)),
        Orientations.Diagonal => new OrientedlLine(
          type,
          orientation,
          DirectionalLine.FromBoard(board, x, y, type, Directions.UpLeft, maxTile, blankTolerance),
          DirectionalLine.FromBoard(board, x, y, type, Directions.DownRight, maxTile, blankTolerance)),
        Orientations.RvDiagonal => new OrientedlLine(
          type,
          orientation,
          DirectionalLine.FromBoard(board, x, y, type, Directions.UpRight, maxTile, blankTolerance),
          DirectionalLine.FromBoard(board, x, y, type, Directions.DownLeft, maxTile, blankTolerance)),
        _ => throw new InvalidOperationException("Unexpected value"),
      };
    }

    /// <summary>
    /// All <see cref="DirectionalLine.BlankTiles"/> from <see cref="Lines"/>.
    /// </summary>
    public IEnumerable<Tile> GetBlankTiles()
    {
      return Lines.SelectMany(l => l.BlankTiles);
    }

    /// <summary>
    /// All <see cref="DirectionalLine.BlockTiles"/> from <see cref="Lines"/>.
    /// </summary>
    public IEnumerable<Tile> GetBlockTiles()
    {
      return Lines.SelectMany(l => l.BlockTiles);
    }

    /// <summary>
    /// All <see cref="DirectionalLine.SameTiles"/> from <see cref="Lines"/>.
    /// </summary>
    public IEnumerable<Tile> GetSameTiles()
    {
      return Lines.SelectMany(l => l.SameTiles);
    }

    /// <summary>
    /// All <see cref="DirectionalLine.Tiles"/> from <see cref="Lines"/>.
    /// </summary>
    public IEnumerable<Tile> GetTiles()
    {
      return Lines.SelectMany(l => l.Tiles);
    }

    public string ToString(bool includesSelf)
    {
      var firstLineString =
        FirstLine.ToString(includesSelf: false, followsLTRB: true);

      var secondLineString =
        SecondLine.ToString(includesSelf: false, followsLTRB: true);

      return includesSelf
        ? string.Join(
          ',',
          firstLineString,
          Piece.ToString(),
          secondLineString)
        : string.Join(
          ',',
          firstLineString,
          secondLineString);
    }

    public override string ToString()
    {
      return ToString(true);
    }
  }
}