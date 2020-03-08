using Gomoku.BoardNS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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