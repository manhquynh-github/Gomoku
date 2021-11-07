using System;
using System.Collections.Generic;
using System.Linq;

using Gomoku.AI.Custom.CandidateSearcher;
using Gomoku.AI.Custom.Evaluator;
using Gomoku.Logic;
using Gomoku.Logic.AI;

namespace Gomoku.AI.Custom.Algorithms
{
  public class GomokuAIAbpMinimax : GomokuAIBase
  {
    public GomokuAIAbpMinimax(int level = 1)
    {
      Level = level;
      TileEvaluator = new WeightedPointBasedEvaluator();
      CandidateSearcher = new LineBasedCandidates();
    }

    /// <summary>
    /// The level of the AI, or more specifically, depth of AI
    /// </summary>
    public int Level { get; set; }

    private ICandidateSearcher CandidateSearcher { get; set; }
    private ITileEvaluator TileEvaluator { get; set; }

    protected override AnalysisResult DoAnalyze(Game clonedGame)
    {
      // If game is over then stop
      if (clonedGame.IsOver)
      {
        return null;
      }

      // Get all the placed tiles to determine all the correct playable tiles
      IReadOnlyList<Tile> placedTiles = clonedGame.History;

      // If it is a new game, select the center most
      if (placedTiles.Count == 0)
      {
        return new AnalysisResult(
          clonedGame.Board[clonedGame.Board.Width / 2, clonedGame.Board.Height / 2]);
      }

      // Get hold of who player this analysis should be done for
      Player forPlayer = clonedGame.CurrentPlayer;

      // Generate nodes for all possible moves
      var possiblePositions = CandidateSearcher.Search(clonedGame).ToList();

      // make a list of pairs of positions - value
      var evaluations = new List<(IPositional positional, double value)>();

      // keep a record of max value
      var max = double.MinValue;

      // evaluate each possible position
      foreach (IPositional position in possiblePositions)
      {
        // play the board to get the next game state
        clonedGame.Play(position);

        // Evaluate the position using the algorithm
        // Note: because the possible position represents the maximizing node,
        // the next node should be done in minimizing phase
        var value = EvaluateMinimax(clonedGame, position, forPlayer, Level, isMaximizing: false);

        // If the value has reached max value, no need to evaluate further
        if (value == double.MaxValue)
        {
          return new AnalysisResult(position);
        }

        // Update the evaluations and max value accordingly
        if (value > max)
        {
          evaluations.Clear();
          evaluations.Add((position, value));
          max = value;
        }
        else if (value == max)
        {
          evaluations.Add((position, value));
        }

        // Evaluation is done, undo the game state
        clonedGame.Undo();
      }

      // Take out all the best choices
      var choices =
        (from evaluation in evaluations
         select evaluation.positional)
         .ToList();

      // Randomly pick one result from the choices
      var choice = Random.Next(choices.Count);
      return new AnalysisResult(choices[choice], choices);
    }

    private double EvaluateGame(Game game, IPositional positional, Player forPlayer, Player againstPlayer)
    {
      var value = TileEvaluator.Evaluate(game, positional, againstPlayer.Piece);

      if (againstPlayer != forPlayer)
      {
        value = -value;
      }

      return value;
    }

    private double EvaluateMinimax(
      Game game,
      IPositional positional,
      Player forPlayer,
      int depth,
      double alpha = double.MinValue,
      double beta = double.MaxValue,
      bool isMaximizing = true)
    {
      // If is leaf node or game is over, evaluate it
      if (depth == 0 || game.IsOver)
      {
        // The leaf node is effectively created by the previous player so it
        // makes sense to evaluate it for the previous player
        return EvaluateGame(game, positional, forPlayer, game.PreviousPlayer);
      }

      // Get all the playable tiles
      IEnumerable<IPositional> playableTiles = CandidateSearcher.Search(game);

      if (isMaximizing)
      {
        var maxValue = double.MinValue;

        // Populate deeper nodes with each playable tile found
        foreach (Tile childTile in playableTiles)
        {
          // play the board to get the next game state
          game.Play(childTile);

          // Prepare to evaluate the next state by getting the next position and
          // recursively evaluate it
          var value = EvaluateMinimax(game, childTile, forPlayer, depth - 1, alpha, beta, false);

          // Evaluation is done, undo the game state
          game.Undo();

          // Negative inifity signifies a lose to our team. Marking this node as
          // negative infinity so that the previous minimizing phase will not
          // pick this node
          if (value == double.MinValue)
          {
            return value;
          }

          // Perform algorithmic updates
          maxValue = Math.Max(maxValue, value);
          alpha = Math.Max(alpha, value);
          if (beta <= alpha)
          {
            return maxValue;
          }
        }
        return maxValue;
      }
      else
      {
        var minValue = double.MaxValue;

        // Populate deeper nodes with each playable tile found
        foreach (Tile childTile in playableTiles)
        {
          // play the board to get the next game state
          game.Play(childTile);

          // Prepare to evaluate the next state by getting the next position and
          // recursively evaluate it
          var value = EvaluateMinimax(game, childTile, forPlayer, depth - 1, alpha, beta, true);

          // Evaluation is done, undo the game state
          game.Undo();

          // Positive inifity signifies a win to our team. Marking this node as
          // positive infinity so that the previous maximizing phase will pick
          // this node
          if (value == double.MaxValue)
          {
            return value;
          }

          // Perform algorithmic updates
          minValue = Math.Min(minValue, value);
          beta = Math.Min(beta, value);
          if (beta <= alpha)
          {
            return minValue;
          }
        }
        return minValue;
      }
    }
  }
}