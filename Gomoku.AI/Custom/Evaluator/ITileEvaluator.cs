using Gomoku.Logic;

namespace Gomoku.AI.Custom.Evaluator
{
  /// <summary>
  /// Represents an interface where a class can be used to evaluate the state of
  /// the <see cref="Game"/> for Gomoku algorithms to use as a starting point.
  /// </summary>
  public interface ITileEvaluator
  {
    /// <summary>
    /// Evaluates the current <paramref name="game"/>'s state for
    /// <paramref name="piece"/> at <paramref name="positional"/> position.
    /// </summary>
    /// <param name="game">the <see cref="Game"/> to be evaluated.</param>
    /// <param name="positional">the <see cref="IPositional"/> to evaluate.</param>
    /// <param name="piece">the <see cref="Piece"/> to evaluate for.</param>
    /// <returns>a score that determines the evaluation.</returns>
    public double Evaluate(Game game, IPositional positional, Piece piece);
  }
}