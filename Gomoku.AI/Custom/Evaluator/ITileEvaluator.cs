using Gomoku.Logic;

namespace Gomoku.AI.Custom.Evaluator
{
  public interface ITileEvaluator
  {
    public double Evaluate(Game game, IPositional positional, Piece piece);
  }
}