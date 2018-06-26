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

        public int CountChainTiles()
        {
            return Lines.Sum(l => l.ChainTiles.Count);
        }

        public int CountBlankTiles()
        {
            return Lines.Sum(l => l.BlankTiles.Count);
        }

        public int CountBlockTiles()
        {
            return Lines.Sum(l => l.BlockTiles.Count);
        }

        public IReadOnlyList<Tile> GetChainTiles()
        {
            List<Tile> tiles = new List<Tile>();
            tiles.AddRange(Lines.SelectMany(l => l.ChainTiles));
            return tiles;
        }

        public IReadOnlyList<Tile> GetBlankTiles()
        {
            List<Tile> tiles = new List<Tile>();
            tiles.AddRange(Lines.SelectMany(l => l.BlankTiles));
            return tiles;
        }

        public IReadOnlyList<Tile> GetBlockTiles()
        {
            List<Tile> tiles = new List<Tile>();
            tiles.AddRange(Lines.SelectMany(l => l.BlockTiles));
            return tiles;
        }
    }
}
