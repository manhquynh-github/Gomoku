using System.Collections.Generic;
using System.Linq;

namespace Gomoku.BoardNS
{
  public class LineGroup
  {
    public LineGroup(IEnumerable<Line> lines)
    {
      Lines = new List<Line>();
      Lines.AddRange(lines);
    }

    public LineGroup(params Line[] lines)
    {
      Lines = new List<Line>();
      Lines.AddRange(lines);
    }

    public int BlankTileCount => Lines.Sum(l => l.BlankTiles.Count);

    public IReadOnlyList<Tile> BlankTiles
    {
      get
      {
        var tiles = new List<Tile>();
        tiles.AddRange(Lines.SelectMany(l => l.BlankTiles));
        return tiles;
      }
    }

    public int BlockTileCount => Lines.Sum(l => l.BlockTiles.Count);

    public IReadOnlyList<Tile> BlockTiles
    {
      get
      {
        var tiles = new List<Tile>();
        tiles.AddRange(Lines.SelectMany(l => l.BlockTiles));
        return tiles;
      }
    }

    public bool IsChained => !Lines.Exists(l => !l.IsChained);
    public List<Line> Lines { get; set; }
    public int SameTileCount => Lines.Sum(l => l.SameTiles.Count);

    public IReadOnlyList<Tile> SameTiles
    {
      get
      {
        var tiles = new List<Tile>();
        tiles.AddRange(Lines.SelectMany(l => l.SameTiles));
        return tiles;
      }
    }
  }
}