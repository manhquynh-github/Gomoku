using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomoku.BoardNS;

namespace Gomoku.AI
{
    public abstract class AbstractGomokuAI
    {
        public abstract Tuple<Tile, IEnumerable<Tile>> Play(Board board);
        public virtual async Task<Tuple<Tile, IEnumerable<Tile>>> PlayAsync(Board board)
        {
            return await Task.Run(() => Play(board));
        }
    }
}
