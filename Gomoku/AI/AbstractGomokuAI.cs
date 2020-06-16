using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Gomoku.Logic;

namespace Gomoku.AI
{
  public abstract class AbstractGomokuAI
  {
    public abstract Tuple<Tile, IEnumerable<Tile>> Play(Game game);

    public virtual async Task<Tuple<Tile, IEnumerable<Tile>>> PlayAsync(Game game)
    {
      return await Task.Run(() => Play(game));
    }
  }
}