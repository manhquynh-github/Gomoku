using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.BoardNS
{
    public class Line
    {
        public readonly IReadOnlyList<Tile> SameTiles;
        public readonly IReadOnlyList<Tile> BlankTiles;
        public readonly IReadOnlyList<Tile> BlockTiles;
        public readonly bool IsChained;

        public Line(
            IReadOnlyList<Tile> sameTiles, 
            IReadOnlyList<Tile> blankTiles,
            IReadOnlyList<Tile> blockTiles,
            bool isChained)
        {
            SameTiles = sameTiles;
            BlankTiles = blankTiles;
            BlockTiles = blockTiles;
            IsChained = isChained;
        }
    }
}
