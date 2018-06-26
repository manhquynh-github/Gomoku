using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Board
{
    public class Line
    {
        public readonly IReadOnlyList<Tile> ChainTiles;
        public readonly IReadOnlyList<Tile> BlankTiles;
        public readonly IReadOnlyList<Tile> BlockTiles;

        public Line(
            IReadOnlyList<Tile> chainTiles, 
            IReadOnlyList<Tile> blankTiles,
            IReadOnlyList<Tile> blockTiles)
        {
            ChainTiles = chainTiles;
            BlankTiles = blankTiles;
            BlockTiles = blockTiles;
        }
    }
}
