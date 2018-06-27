using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Board
{
    public class Line
    {
        public readonly IReadOnlyList<Tile> SameTiles;
        public readonly IReadOnlyList<Tile> BlankTiles;
        public readonly IReadOnlyList<Tile> BlockTiles;
        public readonly bool IsChained;

        public Line(
            IReadOnlyList<Tile> chainTiles, 
            IReadOnlyList<Tile> blankTiles,
            IReadOnlyList<Tile> blockTiles,
            bool isChained)
        {
            SameTiles = chainTiles;
            BlankTiles = blankTiles;
            BlockTiles = blockTiles;
            IsChained = isChained;
        }
    }
}
