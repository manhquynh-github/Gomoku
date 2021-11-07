using System;

namespace Gomoku.Logic.AI
{
  public abstract class GomokuAIBase
  {
    protected static readonly Random Random = new Random();

    public AnalysisResult Analyze(Game game)
    {
      Game clonedGame = game.DeepClone();
      return DoAnalyze(clonedGame);
    }

    public void Play(Game game)
    {
      AnalysisResult result = Analyze(game);
      game.Play(result.SelectedChoice);
    }

    protected abstract AnalysisResult DoAnalyze(Game clonedGame);
  }
}