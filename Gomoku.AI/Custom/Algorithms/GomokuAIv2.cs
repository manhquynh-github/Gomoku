using System;
using System.Collections.Generic;
using System.Linq;

using Gomoku.AI.Custom.CandidateSearcher;
using Gomoku.Logic;
using Gomoku.Logic.AI;

namespace Gomoku.AI.Custom.Algorithms
{
  /// <summary>
  /// An algorithm used for playing Gomoku version 1, based on N-Tree.
  /// Advantage: can perform fundamentally well on any scenario with n = 1.
  /// Disadvantage: Very slow when n &gt;= 2
  /// </summary>
  /// <remarks>written by https://github.com/manhquynh-github</remarks>
  public class GomokuAIv2 : GomokuAIv1
  {
    public GomokuAIv2(int level = 3) : base(level)
    {
    }

    /// <summary>
    /// Searches for a suitable tile for the current turn of the board.
    /// </summary>
    /// <param name="clonedGame">the game used for searching</param>
    /// <returns>a <see cref="Tile"/> that the AI selects.</returns>
    protected override AnalysisResult DoAnalyze(Game clonedGame)
    {
      // Initialize the choices
      var choices = new List<IPositional>();

      // If game is over then stop
      if (clonedGame.IsOver)
      {
        return null;
      }

      var result = Search(clonedGame, Level).ToList();

      // If found no result, return null
      if (result.Count == 0)
      {
        // TODO Play a random tile
        throw new NotImplementedException();
      }

      // If found one result, return the first
      else if (result.Count == 1)
      {
        return new AnalysisResult(result[0].Tile);
      }

      // Otherwise
      else
      {
        result.Sort((x, y) => -1 * x.CompareTo(y));

        // Find the max point of all the nodes, then add all nodes with the max
        // point to the choice collection
        var maxpoint = result[0].Point;
        List<AINode>.Enumerator enumerator = result.GetEnumerator();
        while (enumerator.MoveNext()
          && enumerator.Current.Point == maxpoint)
        {
          choices.Add(enumerator.Current.Tile);
        }

        // Call GC to free up memory from other nodes
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Randomly pick one result from the choices
        var choice = Random.Next(choices.Count);
        return new AnalysisResult(choices[choice], choices);
      }
    }

    protected IEnumerable<AINode> Search(Game game, int level)
    {
      // Get all the placed tiles to determine all the correct playable tiles
      IReadOnlyList<Tile> placedTiles = game.History;

      // If it is a new game, select the center most
      if (placedTiles.Count == 0)
      {
        return new List<AINode>()
        {
          new AINode(
            game.Board[game.Board.Width / 2, game.Board.Height / 2],
            0)
        };
      }

      // Get current player to determine which side to search for
      Player player = game.Manager.CurrentPlayer;
      var playerCount = game.Manager.Players.Length;

      var maxPoint = double.MinValue;
      var candidateNodes = new List<AINode>();

      // Get all the playable tiles
      IEnumerable<IPositional> playableTiles = CandidateSearcher.Search(game);

      // Populate corresponding NTrees with each playable tile found.
      foreach (Tile tile in playableTiles)
      {
        // Play the new cloned board
        game.Play(tile.X, tile.Y);

        // Evaluate this tile
        var point = TileEvaluator.Evaluate(game, tile, player.Piece);
        point = Math.Abs(point);
        var aiNode = new AINode(tile, point);

        if (level < Level)
        {
          if (aiNode.Point > maxPoint)
          {
            maxPoint = aiNode.Point;
            candidateNodes.Clear();
            candidateNodes.Add(aiNode);
          }
          else if (aiNode.Point == maxPoint)
          {
            candidateNodes.Add(aiNode);
          }
          else
          {
            continue;
          }
        }
        else
        {
          candidateNodes.Add(aiNode);
        }

        game.Undo();
      }

      if (level == Level && placedTiles.Count <= 2)
      {
        return candidateNodes;
      }

      if (level == 0)
      {
        return candidateNodes;
      }

      foreach (AINode node in candidateNodes)
      {
        if (node.Point >= 1000.0)
        {
          return candidateNodes;
        }

        var childNodes = Search(game, level - 1).ToList();

        childNodes.Sort((x, y) => -1 * x.CompareTo(y));
        AINode maxNode = childNodes.First();
        childNodes.RemoveAll((x) => x.Point < maxNode.Point);

        // If the current node's board's game is over, stop evaluating because
        // there is a chance this node will reach the end of game, so there is
        // no longer any need to continue evaluating
        //if (b.IsOver)
        //{
        //  break;
        //}

        node.Point -= maxNode.Point;

        // Minus the current node's point by the max point so if the children
        // node's point is high, this node is less likely to be selected.
        node.Point -= 0.01 * childNodes.Count;
      }

      candidateNodes.Sort((x, y) => -1 * x.CompareTo(y));
      AINode maxNode2 = candidateNodes.First();
      candidateNodes.RemoveAll((x) => x.Point < maxNode2.Point);

      return candidateNodes;
    }
  }
}