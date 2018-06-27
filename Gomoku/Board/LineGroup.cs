using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Board
{
    public class LineGroup
    {
        public List<Line> Lines { get; set; }

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

        public int SameTileCount => Lines.Sum(l => l.SameTiles.Count);
        public int BlankTileCount => Lines.Sum(l => l.BlankTiles.Count);
        public int BlockTileCount => Lines.Sum(l => l.BlockTiles.Count);
        public bool IsChained => !Lines.Exists(l => !l.IsChained);

        public IReadOnlyList<Tile> SameTiles
        {
            get
            {
                List<Tile> tiles = new List<Tile>();
                tiles.AddRange(Lines.SelectMany(l => l.SameTiles));
                return tiles;
            }
        }

        public IReadOnlyList<Tile> BlankTiles
        {
            get
            {
                List<Tile> tiles = new List<Tile>();
                tiles.AddRange(Lines.SelectMany(l => l.BlankTiles));
                return tiles;
            }
        }

        public IReadOnlyList<Tile> BlockTiles
        {
            get
            {
                List<Tile> tiles = new List<Tile>();
                tiles.AddRange(Lines.SelectMany(l => l.BlockTiles));
                return tiles;
            }
        }

    }
}
