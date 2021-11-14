using System.Collections.Generic;

using Gomoku.Logic;

namespace Gomoku.AI.Custom.Algorithms
{
  /// <summary>
  /// Represents an interface where a class can be used to search for candidate
  /// tiles for Gomoku algorithms to use as a starting point.
  /// </summary>
  public interface ICandidateSearcher
  {
    /// <summary>
    /// Search for playable tiles given a <paramref name="game"/>.
    /// </summary>
    /// <param name="game">the <see cref="Game"/> to search for.</param>
    /// <returns>a collection of candidate positions resulting from the search.</returns>
    public IEnumerable<IPositional> Search(Game game);
  }
}