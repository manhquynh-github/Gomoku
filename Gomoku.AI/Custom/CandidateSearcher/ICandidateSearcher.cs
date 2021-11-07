using System.Collections.Generic;

using Gomoku.Logic;

namespace Gomoku.AI.Custom.Algorithms
{
  public interface ICandidateSearcher
  {
    public IEnumerable<IPositional> Search(Game game);
  }
}