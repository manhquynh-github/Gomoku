using System;
using System.Collections.Generic;
using System.Linq;

using Gomoku.AI.Common.DataStructures;
using Gomoku.AI.Custom.CandidateSearcher;
using Gomoku.AI.Custom.Evaluator;
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
  public class GomokuAIv1 : GomokuAIBase
  {
    public GomokuAIv1(int level = 1)
    {
      Level = level;
      TileEvaluator = new WeightedPointBasedEvaluator();
      CandidateSearcher = new LineBasedCandidates();
    }

    /// <summary>
    /// The level of the AI, or more specifically, depth of AI
    /// </summary>
    public int Level { get; set; }

    protected ICandidateSearcher CandidateSearcher { get; private set; }
    protected ITileEvaluator TileEvaluator { get; private set; }

    /// <summary>
    /// Searches for a suitable tile for the current turn of the board.
    /// </summary>
    /// <param name="clonedGame">the game used for searching</param>
    /// <returns>a <see cref="Tile"/> that the AI selects.</returns>
    protected override AnalysisResult DoAnalyze(Game clonedGame)
    {
      // If game is over then stop
      if (clonedGame.IsOver)
      {
        return null;
      }

      // Initialize the choices
      var choices = new List<IPositional>();

      List<NTree<AINode>> result = Search(clonedGame, null, Level);

      // Print out to console for debugging
      //PrintSearchResult(result);

      // If found no result, return null
      if (result.Count == 0)
      {
        // TODO Play a random tile
        throw new NotImplementedException();
      }

      // If found one result, return the first
      else if (result.Count == 1)
      {
        return new AnalysisResult(result[0].Value.Tile);
      }

      // Otherwise
      else
      {
        // Find the max point of all the nodes, then add all nodes with the max
        // point to the choice collection
        var maxpoint = result[0].Value.Point;
        List<NTree<AINode>>.Enumerator enumerator = result.GetEnumerator();
        while (enumerator.MoveNext()
          && enumerator.Current.Value.Point == maxpoint)
        {
          choices.Add(enumerator.Current.Value.Tile);
        }

        // Call GC to free up memory from other nodes
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Randomly pick one result from the choices
        var choice = Random.Next(choices.Count);
        return new AnalysisResult(choices[choice], choices);
      }
    }

    protected void PrintSearchResult(List<NTree<AINode>> nTrees)
    {
      foreach (NTree<AINode> node in nTrees)
      {
        Console.WriteLine(node.Value.Tile.X + "," + node.Value.Tile.Y + " = " + node.Value.Point);
      }
      Console.WriteLine("--------------");
    }

    protected List<NTree<AINode>> Search(Game game, NTree<AINode> currentNode, int level)
    {
      // Get all the placed tiles to determine all the correct playable tiles
      IReadOnlyList<Tile> placedTiles = game.History;

      // If it is a new game, select the center most
      if (placedTiles.Count == 0)
      {
        return new List<NTree<AINode>>()
        {
          new NTree<AINode>(
            new AINode(
              game.Board[game.Board.Width / 2, game.Board.Height / 2],
              0))
        };
      }

      // Get all the playable tiles
      IEnumerable<IPositional> playableTiles = CandidateSearcher.Search(game);

      // Get current player to determine which side to search for
      Player player = game.Manager.CurrentPlayer;
      var playerCount = game.Manager.Players.Length;

      // Populate corresponding NTrees with each playable tile found.
      var nTrees = new List<NTree<AINode>>();
      foreach (Tile tile in playableTiles)
      {
        // Play the board
        game.Play(tile.X, tile.Y);

        // Evalue this tile
        var point = TileEvaluator.Evaluate(game, tile, player.Piece);
        point = Math.Abs(point);
        var aiNode = new AINode(tile, point);

        // Add to the list of NTrees
        var nTree = new NTree<AINode>(currentNode, aiNode);
        nTrees.Add(nTree);

        // If the current node's board's game is over, stop evaluating because
        // there is a chance this node will reach the end of game, so there is
        // no longer any need to continue evaluating
        if (game.IsOver)
        {
          game.Undo();
          break;
        }

        // If the recursion of searching didn't reach the bottom (where level ==
        // 0) then, keep evaluating current node by evaluating its children
        // nodes, where the children's side is the next player's side.
        if (level > 0 && level <= Level)
        {
          // Evaluate children nodes by recursion
          nTree.Nodes = Search(game, nTree, level - 1);

          if (nTree.Nodes.Count > 0)
          {
            // Get max point of the children nodes Take the first node because
            // it is sorted by descending
            NTree<AINode> firstNode = nTree.Nodes.First();
            var maxPoint = firstNode.Value.Point;

            // Minus the current node's point by the max point so if the
            // children node's point is high, this node is less likely to be
            // selected. use j as a factor for the point as it goes shallower
            // back to its parent right before the original player's turn again
            NTree<AINode> traverseNode = nTree;
            for (var j = 1; j < playerCount && !(traverseNode is null); j++)
            {
              traverseNode.Value.Point -= j * maxPoint;
              traverseNode = traverseNode.ParentNode;
            }

            // Remove all chilren nodes with lower points
            nTree.Nodes.RemoveAll(n => n.Value.Point < maxPoint);

            // Call GC to free up memory
            //GC.Collect();
            //GC.WaitForPendingFinalizers();

            // The more the chilren nodes are left, the less likely the node is
            // to be selected. use j as a factor for the point as it goes
            // shallower back to its parent right before the original player's
            // turn again use 0.01 as a small factor to penalize same nodes left
            traverseNode = nTree;
            for (var j = 1; j < playerCount && !(traverseNode is null); j++)
            {
              traverseNode.Value.Point -= j * nTree.Nodes.Count * 0.01;
              traverseNode = traverseNode.ParentNode;
            }
          }
        }

        // Evaludation is done, undo the step
        game.Undo();
      }

      // Sort the NTrees list by descending where the first item has the largest point
      nTrees.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));
      return nTrees;
    }

    protected class AINode : IComparable<AINode>
    {
      public double Point;
      public Tile Tile;

      public AINode(Tile tile, double point)
      {
        Tile = tile;
        Point = point;
      }

      public int CompareTo(AINode other)
      {
        var value = Point - other.Point;

        return value == 0.0
          ? 0
          : value > 0.0
          ? 1
          : -1;
      }
    }
  }
}