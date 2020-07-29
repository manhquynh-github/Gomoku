using System;

namespace Gomoku.Logic.AI
{
  public abstract class GomokuAIBase
  {
    protected static readonly Random Random = new Random();

    public abstract AnalysisResult Analyze(Game game);

    public Tile Play(Game game)
    {
      AnalysisResult result = Analyze(game);
      return result.SelectedChoice;
    }
  }
}