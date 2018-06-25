using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    public class Board
    {
        public readonly int Height;
        public readonly int Width;
        public readonly Tile[,] Tiles;
        public readonly List<Piece> Pieces;
        private int _turn;
        public int Turn
        {
            get => _turn;
            set
            {
                _turn = Turn;
                TurnChanged?.Invoke(this, new TurnChangedEventArgs(_turn, Pieces[_turn]));
            }
        }

        public event TurnChangedEventHandler TurnChanged;

        public Board(int height, int width, IList<Piece> pieces)
        {
            if (height <= 5 || width <= 5)
            {
                throw new ArgumentException();
            }

            Height = height;
            Width = width;
            Tiles = new Tile[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Tiles[i, j] = new Tile(i, j);
            Pieces = new List<Piece>(pieces);
            Turn = 0;
        }

        public IList<Tile> GetSurroundingTiles(Tile tile, Direction direction)
        {
            List<Tile> tiles = new List<Tile>();
            char symbol = tile.Piece.Symbol;

            bool fnAddTiles(int i, int j)
            {
                tiles.Add(Tiles[i, j]);
                return Tiles[i, j].Piece.Symbol == symbol;
            }

            switch (direction)
            {
                case Direction.Left:
                    for (int i = tile.X - 1, j = tile.Y;
                        i >= 0 && fnAddTiles(i, j);
                        i--) ;
                    break;
                case Direction.Right:
                    for (int i = tile.X + 1, j = tile.Y; 
                        i < Width && fnAddTiles(i, j);
                        i++) ;
                    break;
                case Direction.Up:
                    for (int i = tile.X, j = tile.Y - 1;
                        j >= 0 && fnAddTiles(i, j);
                        j--) ;
                    break;
                case Direction.Down:
                    for (int i = tile.X, j = tile.Y + 1; 
                        j < Height && fnAddTiles(i, j);
                        j++) ;
                    break;
                case Direction.UpLeft:
                    for (int i = tile.X - 1, j = tile.Y - 1;
                        i >= 0 && j >= 0 && fnAddTiles(i, j);
                        i--, j--) ;
                    break;
                case Direction.DownRight:
                    for (int i = tile.X + 1, j = tile.Y + 1;
                        i <= Width && j <= Height && fnAddTiles(i, j);
                        i++, j++) ;
                    break;
                case Direction.UpRight:
                    for (int i = tile.X + 1, j = tile.Y - 1;
                        i < Width && j >= 0 && fnAddTiles(i, j);
                        i++, j--) ;
                    break;
                case Direction.DownLeft:
                    for (int i = tile.X - 1, j = tile.Y + 1;
                        i >= 0 && j < Height && fnAddTiles(i, j);
                        i--, j++) ;
                    break;
            }

            return tiles;
        }

        public IList<Tile> GetSurroundingTiles(Tile tile, Orientation orientation)
        {
            List<Tile> tiles = new List<Tile>();

            switch (orientation)
            {
                case Orientation.Horizontal:
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.Left));
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.Right));
                    break;
                case Orientation.Vertical:
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.Up));
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.Down));
                    break;
                case Orientation.Diagonal:
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.UpLeft));
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.DownRight));
                    break;
                case Orientation.RvDiagonal:
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.UpRight));
                    tiles.AddRange(GetSurroundingTiles(tile, Direction.DownLeft));
                    break;
            }

            return tiles;
        }

        
    }
}
