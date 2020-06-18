﻿using System;
using System.Collections.Generic;
using System.Linq;

using Gomoku.Logic;

namespace Gomoku.AI
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
    /// <param name="game">the game used for searching</param>
    /// <returns>a <see cref="Tile"/> that the AI selects.</returns>
    public override Tuple<Tile, IEnumerable<Tile>> Play(Game game)
    {
      // Initialize the choices
      var choices = new List<Tile>();

      // If game is over then stop
      if (game.IsOver)
      {
        return null;
      }

      var result = Search(game, Level).ToList();

      // If found no result, return null
      if (result.Count == 0)
      {
        return new Tuple<Tile, IEnumerable<Tile>>(null, choices);
      }

      // If found one result, return the first
      else if (result.Count == 1)
      {
        choices.Add(result[0].Tile);
        return new Tuple<Tile, IEnumerable<Tile>>(choices[0], choices);
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
        var choice = App.Random.Next(choices.Count);
        return new Tuple<Tile, IEnumerable<Tile>>(choices[choice], choices);
      }
    }

    protected IEnumerable<AINode> Search(Game game, int level)
    {
      // Get all the placed tiles to determine all the correct playable tiles
      Stack<Tile> placedTiles = game.History;

      // If it is a new game, select the center most
      if (placedTiles.Count == 0)
      {
        return new List<AINode>()
        {
          new AINode(
            game.Board[game.Width / 2, game.Height / 2],
            game,
            0)
        };
      }

      // Get current player to determine which side to search for
      Player player = game.GetCurrentPlayer();
      var playerCount = game.Players.Count;

      var maxPoint = double.MinValue;
      var candidateNodes = new List<AINode>();

      // Populate corresponding NTrees with each playable tile found.
      foreach (Tile tile in GetPlayableTiles(game))
      {
        // Clone the current board to create a new state of NTree
        var g = game.DeepClone();

        // Play the new cloned board
        g.Play(tile);

        // Evaluate this tile
        AINode aiNode = EvaluatePoint(g, tile, player.Piece.Type);

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

        var childNodes = Search(node.Game, level - 1).ToList();

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