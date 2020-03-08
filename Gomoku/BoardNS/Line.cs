using System.Collections.Generic;

namespace Gomoku.BoardNS
{
  public class Line
  {
    public static readonly Line EMPTY = new Line(new List<Tile>());

    public Line(IEnumerable<Tile> sameTiles, bool isChained)
    {
      SameTiles = new List<Tile>(sameTiles);
      IsChained = isChained;
    }

    public Line(IEnumerable<Tile> sameTiles) :
      this(sameTiles, false)
    {
    }

    public Line(
      IEnumerable<Tile> sameTiles,
      IEnumerable<Tile> blankTiles,
      IEnumerable<Tile> blockTiles,
      bool isChained)
    {
      SameTiles = new List<Tile>(sameTiles);
      BlankTiles = new List<Tile>(blankTiles);
      BlockTiles = new List<Tile>(blockTiles);
      IsChained = isChained;
    }

    public IReadOnlyList<Tile> BlankTiles { get; }
    public IReadOnlyList<Tile> BlockTiles { get; }
    public bool IsChained { get; }
    public IReadOnlyList<Tile> SameTiles { get; }
  }
}