using System;
using System.Linq;
using System.Threading.Tasks;

using Gomoku.Logic;

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