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
        public int Level { get; set; } = 1;

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

        protected List<NTree<AINode>> Search(Board.Board board, int level)
        {
            Player player = board.GetCurrentPlayer();

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
                    playableTiles.AddRange(board.GetLineGroup(tile, (Orientation)i, Piece.EMPTY, 2).GetChainTiles());
                }
            }

            List<NTree<AINode>> nTrees = new List<NTree<AINode>>();
            foreach (var tile in playableTiles)
            {
                Board.Board b = board.Clone() as Board.Board;
                b.Play(tile);

                double point = 0.0;

                if (b.IsGameOver)
                {
                    point = 100.0;
                }
                else
                {
                    for (int i = 0; i <= 3; i++)
                    {
                        LineGroup lineGroup = board.GetLineGroup(tile, (Orientation)i, player.Piece, 4);
                        int chainTilesCount = lineGroup.CountChainTiles();
                        int blockTilesCount = lineGroup.CountBlockTiles();
                        if (blockTilesCount == 0)
                        {
                            double _point = (1.0 * (1.0 - Math.Pow(2.0, chainTilesCount)) / (1.0 - 2.0));
                            point += _point * _point;
                        }
                        else if (blockTilesCount == 1)
                        {
                            point += chainTilesCount;
                        }




                        //point += (1.0 * (1.0 - Math.Pow(2.0, lineGroup.CountChainTiles())) / (1.0 - 2.0));
                        //point += 0.25 * lineGroup.CountBlankTiles();
                        //point -= 1.0 * lineGroup.CountBlockTiles();
                    }
                }

                AINode node = new AINode(tile, b, point);
                NTree<AINode> nTree = new NTree<AINode>(node);
                nTrees.Add(nTree);

                if (b.IsGameOver)
                    break;

                if (level > 0 && level <= Level)
                {
                    nTree.Nodes = Search(b, level - 1);
                    double maxpoint = nTree.Nodes.Max(n => n.Value.Point);
                    node.Point -= maxpoint;
                    node.Point -= 0.01 * nTree.Nodes.Where(n => n.Value.Point == maxpoint).Count();

                    //node.Point -= nTree.Nodes.Sum(n => n.Value.Point);
                }
            }

            nTrees.Sort((x, y) => -1 * x.Value.CompareTo(y.Value));
            return nTrees;
        }

        public Tile Play(Board.Board board, out List<Tile> choices)
        {
            choices = new List<Tile>();

            if (board.IsGameOver)
                return null;

            var result = Search(board, 1);
            //foreach (var node in result)
            //{
            //    Console.WriteLine(node.Value.Tile.X + "," + node.Value.Tile.Y + " = " + node.Value.Point);
            //}
            //Console.WriteLine("--------------");


            if (result.Count == 0)
            {
                return null;
            }
            else if (result.Count == 1)
            {
                choices.Add(result[0].Value.Tile);
                return choices[0];
            }
            else
            {
                double maxpoint = result[0].Value.Point;
                var enumerator = result.GetEnumerator();
                while (enumerator.MoveNext()
                    && enumerator.Current.Value.Point == maxpoint)
                {
                    choices.Add(enumerator.Current.Value.Tile);
                }

                Random random = new Random();
                int choice = random.Next(choices.Count);
                return choices[choice];
            }
        }

    }
}
