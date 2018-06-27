using System;
using System.Collections.Generic;
using System.Linq;
using Gomoku.Board;

namespace Gomoku.AI
{
    // The AI used for playing Gomoku version 1
    public class GomokuAIv1
    {
        /// <summary>
        /// The level of the AI, or more specifically, depth of AI
        /// </summary>
        public int Level { get; set; }

        public GomokuAIv1(int level = 1)
        {
            Level = level;
        }

        protected class AINode : IComparable<AINode>
        {
            public Tile Tile;
            public Board.Board Board;
            public double Point;

            public AINode(Tile tile, Board.Board board, double point)
            {
                Tile = tile;
                Board = board;
                Point = point;
            }

            public int CompareTo(AINode other)
            {
                double diff = Point - other.Point;
                if (diff == 0.0)
                    return 0;
                else if (diff > 0.0)
                    return 1;
                else
                    return -1;
            }
        }

        protected List<NTree<AINode>> Search(Board.Board board, Player originalPlayer, int level)
        {
            // Get current player to determine 
            // which side to search for
            Player player = board.GetCurrentPlayer();

            // Get all the placed tiles to determine
            // all the correct playable tiles
            List<Tile> placedTiles = new List<Tile>();
            foreach (var tile in board.Tiles)
            {
                if (tile.Piece != Piece.EMPTY)
                    placedTiles.Add(tile);
            }

            // Get all the playable tiles using a HashSet
            // where only distinct tiles are added
            HashSet<Tile> playableTiles = new HashSet<Tile>();
            foreach (var tile in placedTiles)
            {
                // Loop all 4 Orientation enumeration
                for (int i = 0; i <= 3; i++)
                {
                    // Retrieve LineGroup of each orientation
                    // within 2-tile range where the tiles are empty
                    foreach (var t in 
                        board.GetLineGroup(
                            tile, 
                            (Orientation)i, 
                            Piece.EMPTY, 2).SameTiles)
                    {
                        playableTiles.Add(t);
                    }
                }
            }

            // Populate corresponding NTrees with each
            // playable tile found.
            List<NTree<AINode>> nTrees = new List<NTree<AINode>>();
            foreach (var tile in playableTiles)
            {
                // Clone the current board to create
                // a new state of NTree
                Board.Board b = board.Clone() as Board.Board;

                // Play the new cloned board
                b.Play(tile);

                // Evaluate the point of the current node
                double point = 0.0;

                // If game is over the point should be high enough
                // so that this node is more likely to get noticed
                if (b.IsGameOver)
                {
                    point = 100.0;
                }
                // Otherwise evaluate the point matching lines
                // and other information
                else
                {
                    // Retrieve line group of each orientation
                    // to fully evaluate a tile
                    for (int i = 0; i <= 3; i++)
                    {
                        // Get LineGroup within 5-tile range
                        LineGroup lineGroup = 
                            board.GetLineGroup(
                                tile, (Orientation)i,
                                player.Piece,
                                5);
                        
                        // Calculate points
                        int sameTilesCount = lineGroup.SameTileCount;
                        int blockTilesCount = lineGroup.BlockTileCount;

                        // When the line is not blocked
                        if (blockTilesCount == 0)
                        {
                            // If the line chain has more tiles than win pieces,
                            // then this tile is less worth.
                            if (sameTilesCount + 1 >= Board.Board.WINPIECES)
                            {
                                point += sameTilesCount;
                            }
                            // Otherwise
                            else
                            {
                                // Calculate point using Geometric series of 2.0
                                // so that the more chain it has, the more
                                // valuable the line
                                double _point =
                                    (1.0 * (1.0 - Math.Pow(2.0, sameTilesCount)) / (1.0 - 2.0));

                                // Finally the point is added with the
                                // power of itself
                                point += Math.Pow(_point, lineGroup.IsChained ? 2.0 : 1.5);
                            }
                        }
                        // When the line is partially blocked,
                        // only add the point which equals to
                        // the same count
                        else if (blockTilesCount == 1)
                        {
                            point += sameTilesCount;
                        }
                        // Otherwise, add no point.

                        //point += (1.0 * (1.0 - Math.Pow(2.0, lineGroup.CountChainTiles())) / (1.0 - 2.0));
                        //point += 0.25 * lineGroup.CountBlankTiles();
                        //point -= 1.0 * lineGroup.BlockTileCount;
                    }
                }

                // Instatiate an AINode containing all of
                // the above information
                AINode node = new AINode(tile, b, point);
                NTree<AINode> nTree = new NTree<AINode>(node);

                // Add to the list of NTrees
                nTrees.Add(nTree);

                // If the current node's board's game is over,
                // stop evaluating because there is a chance
                // this node will reach the end of game, so
                // there is no longer any need to continue evaluating
                if (b.IsGameOver)
                    break;

                // If the recursion of searching didn't reach
                // the bottom (where level == 0) then, keep
                // evaluating current node by evaluating its
                // children nodes, where the children's side is
                // the next player's side.
                if (level > 0 && level <= Level)
                {
                    // Evaluate children nodes by recursion
                    nTree.Nodes = Search(b, originalPlayer, level - 1);

                    if (nTree.Nodes.Count > 0)
                    {
                        // Get max point of the children nodes
                        double maxPoint = nTree.Nodes.First().Value.Point;

                        // Minus the current node's point by the max point
                        // so if the children node's point is high,
                        // this node is less likely to be selected.
                        node.Point -= maxPoint;

                        // Remove all chilren nodes with lower points
                        nTree.Nodes.RemoveAll(n => n.Value.Point < maxPoint);

                        // Call GC to free up memory
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // The more the chilren nodes are left,
                        // the less likely the node is to be selected.
                        node.Point -= 0.01 * nTree.Nodes.Count;
                    }
                }
            }

            // Sort the NTrees list by descending
            // where the first item has the largest point
            nTrees.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));
            return nTrees;
        }

        /// <summary>
        /// Searches for a suitable tile for the current turn of the board.
        /// </summary>
        /// <param name="board">the board used for searching</param>
        /// <param name="choices">all the results of the search</param>
        /// <returns>a <see cref="Tile"/> that the AI selects.</returns>
        public Tile Play(Board.Board board, out List<Tile> choices)
        {
            // Initialize the choices
            choices = new List<Tile>();

            // If game is over then stop            
            if (board.IsGameOver)
                return null;

            var result = Search(board, board.GetCurrentPlayer(), 1);

            // Print out to console for debugging
            //PrintSearchResult(result);

            // If found no result, return null
            if (result.Count == 0)
            {
                return null;
            }
            // If found one result, return the first
            else if (result.Count == 1)
            {
                choices.Add(result[0].Value.Tile);
                return choices[0];
            }
            // Otherwise
            else
            {
                // Find the max point of all the nodes,
                // then add all nodes with the max point to
                // the choice collection
                double maxpoint = result[0].Value.Point;
                var enumerator = result.GetEnumerator();
                while (enumerator.MoveNext()
                    && enumerator.Current.Value.Point == maxpoint)
                {
                    choices.Add(enumerator.Current.Value.Tile);
                }

                // Call GC to free up memory from other nodes
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Randomly pick one result from the choices
                int choice = App.Random.Next(choices.Count);
                return choices[choice];
            }
        }

        protected void PrintSearchResult(List<NTree<AINode>> nTrees)
        {
            foreach (var node in nTrees)
            {
                Console.WriteLine(node.Value.Tile.X + "," + node.Value.Tile.Y + " = " + node.Value.Point);
            }
            Console.WriteLine("--------------");
        }
    }
}
