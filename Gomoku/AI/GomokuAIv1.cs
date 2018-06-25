using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomoku.Board;

namespace Gomoku.AI
{
    public class GomokuAIv1
    {
        public GomokuAIv1()
        {
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
                return (int)(Point - other.Point);
            }
        }

        public Tile Play(Board.Board board, Player player)
        {
            List<Tile> placedTiles = new List<Tile>();
            foreach (var tile in board.Tiles)
            {
                if (tile.Piece != Piece.EMPTY)
                    placedTiles.Add(tile);
            }

            List<Tile> playableTiles = new List<Tile>();
            foreach (var tile in placedTiles)
            {
                for (int i = 0; i <= 3; i++)
                {
                    playableTiles.AddRange(board.GetSameTiles(tile, (Orientation)i, Piece.EMPTY, 2, false));
                }
            }

            List<NTree<AINode>> nTrees = new List<NTree<AINode>>();
            foreach (var tile in playableTiles)
            {
                //Board.Board b = board.Clone() as Board.Board;
                //b.Play(tile);

                double point = 0.0;
                for (int i = 0; i <= 3; i++)
                {
                    List<Tile> surroundingTiles = board.GetSameTiles(tile, (Orientation)i, player.Piece, 4, true);
                    int sameCount = surroundingTiles.Where(t => t.Piece == player.Piece).Count();
                    point += (1.0 * (1.0 - Math.Pow(2.0, sameCount) / (1.0 - 2.0)));

                    foreach (var t in surroundingTiles)
                    {
                        if (t.Piece == Piece.EMPTY)
                            point += 0.5;
                        else
                            point -= 1.0;
                    }
                }

                nTrees.Add(new NTree<AINode>(new AINode(tile, null, point)));
            }

            nTrees.Sort((x, y) => x.Value.CompareTo(y.Value));
            return nTrees.Last().Value.Tile;
        }

    }
}
