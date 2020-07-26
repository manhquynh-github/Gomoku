using System;
using System.Collections.Generic;
using System.Linq;

using Gomoku.Logic;
using Gomoku.Logic.AI;

namespace Gomoku.AI
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
    }

    /// <summary>
    /// The level of the AI, or more specifically, depth of AI
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Searches for a suitable tile for the current turn of the board.
    /// </summary>
    /// <param name="game">the game used for searching</param>
    /// <returns>a <see cref="Tile"/> that the AI selects.</returns>
    public override AnalysisResult Analyze(Game game)
    {
      // Initialize the choices
      var choices = new List<Tile>();

      // If game is over then stop
      if (game.IsOver)
      {
        return null;
      }

      List<NTree<AINode>> result = Search(game, null, Level);

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
        choices.Add(result[0].Value.Tile);
        return new AnalysisResult(choices, choices[0]);
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
        return new AnalysisResult(choices, choices[choice]);
      }
    }

    protected AINode EvaluatePoint(Game game, Tile tile, Piece piece)
    {
      // Evaluate the point of the current node
      var point = 0.0;

      // If game is over the point should be high enough so that this node is
      // more likely to get noticed
      if (game.IsOver)
      {
        point = 1000.0;
      }

      // Otherwise evaluate the point matching lines and other information
      else
      {
        // Retrieve line group of each orientation to fully evaluate a tile
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          // Get line within 5-tile range
          var line =
            OrientedlLine.FromBoard(
              game.Board,
              tile.X,
              tile.Y,
              piece,
              orientation,
              maxTile: Game.WINPIECES,
              blankTolerance: 1);

          // Calculate points
          var sameTilesCount = line.SameTileCount;
          var blockTilesCount = line.BlockTilesCount;

          // When the line is not blocked
          if (blockTilesCount == 0)
          {
            // If the line chain has more tiles than win pieces, then this tile
            // is less worth.
            if (sameTilesCount + 1 >= Game.WINPIECES)
            {
              point += sameTilesCount;
            }

            // Otherwise
            else
            {
              // Calculate point using Geometric series of 2.0 so that the more
              // chain it has, the more valuable the line
              var _point =
                  1.0 * (1.0 - Math.Pow(2.0, sameTilesCount)) / (1.0 - 2.0);

              // Finally the point is added with the power of itself
              point += Math.Pow(_point, line.IsChained ? 2.0 : 1.5);
            }
          }

          // When the line is partially blocked, only add the point which equals
          // to the same count
          else if (blockTilesCount == 1)
          {
            point += sameTilesCount;
          }

          // Otherwise, add no point.

          //point += (1.0 * (1.0 - Math.Pow(2.0, line.CountChainTiles())) / (1.0 - 2.0));
          //point += 0.25 * line.CountBlankTiles();
          //point -= 1.0 * line.BlockTileCount;
        }
      }

      // Instatiate an AINode containing all of the above information
      return new AINode(tile, game, point);
    }

    protected IEnumerable<Tile> GetPlayableTiles(Game game)
    {
      // Get all the placed tiles to determine all the correct playable tiles
      Stack<Tile> placedTiles = game.History;

      // Get all the playable tiles using a HashSet where only distinct tiles
      // are added
      var playableTiles = new HashSet<Tile>();
      foreach (Tile tile in placedTiles)
      {
        // Loop all 4 Orientation enumeration
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          // Retrieve line of each orientation within 2-tile range where the
          // tiles are empty
          foreach (Tile t in
            OrientedlLine.FromBoard(
              game.Board,
              tile.X,
              tile.Y,
              (Piece)Pieces.None,
              orientation,
              maxTile: 2,
              blankTolerance: 1)
            .SameTiles)
          {
            playableTiles.Add(t);
          }
        }
      }

      return playableTiles;
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
      Stack<Tile> placedTiles = game.History;

      // If it is a new game, select the center most
      if (placedTiles.Count == 0)
      {
        return new List<NTree<AINode>>()
        {
          new NTree<AINode>(
            new AINode(
              game.Board[game.Width / 2, game.Height / 2],
              game,
              0))
        };
      }

      // Get all the playable tiles
      IEnumerable<Tile> playableTiles = GetPlayableTiles(game);

      // Get current player to determine which side to search for
      Player player = game.CurrentPlayer;
      var playerCount = game.Players.Count;

      // Populate corresponding NTrees with each playable tile found.
      var nTrees = new List<NTree<AINode>>();
      foreach (Tile tile in playableTiles)
      {
        // Clone the current board to create a new state of NTree
        Game g = game.DeepClone();

        // Play the new cloned board
        g.Play(tile.X, tile.Y);

        // Evalue this tile
        AINode aiNode = EvaluatePoint(g, tile, player.Piece);

        // Add to the list of NTrees
        var nTree = new NTree<AINode>(currentNode, aiNode);
        nTrees.Add(nTree);

        // If the current node's board's game is over, stop evaluating because
        // there is a chance this node will reach the end of game, so there is
        // no longer any need to continue evaluating
        if (g.IsOver)
        {
          break;
        }

        // If the recursion of searching didn't reach the bottom (where level ==
        // 0) then, keep evaluating current node by evaluating its children
        // nodes, where the children's side is the next player's side.
        if (level > 0 && level <= Level)
        {
          // Evaluate children nodes by recursion
          nTree.Nodes = Search(g, nTree, level - 1);

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
            for (var j = 1; j < playerCount && traverseNode != null; j++)
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
            for (var j = 1; j < playerCount && traverseNode != null; j++)
            {
              traverseNode.Value.Point -= j * nTree.Nodes.Count * 0.01;
              traverseNode = traverseNode.ParentNode;
            }
          }
        }
      }

      // Sort the NTrees list by descending where the first item has the largest point
      nTrees.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));
      return nTrees;
    }

    protected class AINode : IComparable<AINode>
    {
      public Game Game;
      public double Point;
      public Tile Tile;

      public AINode(Tile tile, Game game, double point)
      {
        Tile = tile;
        Game = game;
        Point = point;
      }

      public int CompareTo(AINode other)
      {
        return (int)(Point - other.Point);
      }
    }
  }
}