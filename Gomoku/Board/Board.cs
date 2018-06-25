using System;
using System.Collections.Generic;
using System.Windows;

namespace Gomoku.Board
{
    public class Board : ICloneable
    {
        public readonly int Height;
        public readonly int Width;
        public readonly Tile[,] Tiles;
        public readonly List<Player> Players;
        private int _turn;
        private bool _isGameOver;

        public static readonly int WINPIECES = 5;

        public int Turn
        {
            get => _turn;
            set
            {
                _turn = value;
                TurnChanged?.Invoke(new TurnChangedEventArgs(this, _turn, GetCurrentPlayer()));
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set
            {
                _isGameOver = value;
                if (value == true)
                {
                    MessageBox.Show(GetCurrentPlayer().Name + " wins!");
                }
            }
        }

        public event TurnChangedEventHandler TurnChanged;

        public Board(int width, int height, IList<Player> players)
        {
            if (height <= 5 || width <= 5)
            {
                throw new ArgumentException();
            }

            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Tiles[i, j] = new Tile(i, j);
            Players = new List<Player>(players);
            Turn = 0;
            IsGameOver = false;
        }

        public List<Tile> GetSameTiles(
            Tile tile,
            Direction direction,
            Piece piece,
            int max = 4,
            bool includeSurrounding = true)
        {
            List<Tile> tiles = new List<Tile>();
            string symbol = piece.Symbol;

            int count = 0;

            bool fnAddTiles(int i, int j)
            {
                if (count == max)
                {
                    count = 0;
                    return false;
                }

                if (Tiles[i, j].Piece.Symbol == symbol)
                {
                    tiles.Add(Tiles[i, j]);
                    count++;
                    return true;
                }
                else
                {
                    if (includeSurrounding)
                        tiles.Add(Tiles[i, j]);

                    count = 0;
                    return false;
                }
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
                        i < Width && j < Height && fnAddTiles(i, j);
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

        public List<Tile> GetSameTiles(
            Tile tile,
            Orientation orientation,
            Piece piece,
            int overflow = 4,
            bool includeSurrounding = true)
        {
            List<Tile> tiles = new List<Tile>();

            switch (orientation)
            {
                case Orientation.Horizontal:
                    tiles.AddRange(GetSameTiles(tile, Direction.Left, piece, overflow, includeSurrounding));
                    tiles.AddRange(GetSameTiles(tile, Direction.Right, piece, overflow, includeSurrounding));
                    break;
                case Orientation.Vertical:
                    tiles.AddRange(GetSameTiles(tile, Direction.Up, piece, overflow, includeSurrounding));
                    tiles.AddRange(GetSameTiles(tile, Direction.Down, piece, overflow, includeSurrounding));
                    break;
                case Orientation.Diagonal:
                    tiles.AddRange(GetSameTiles(tile, Direction.UpLeft, piece, overflow, includeSurrounding));
                    tiles.AddRange(GetSameTiles(tile, Direction.DownRight, piece, overflow, includeSurrounding));
                    break;
                case Orientation.RvDiagonal:
                    tiles.AddRange(GetSameTiles(tile, Direction.UpRight, piece, overflow, includeSurrounding));
                    tiles.AddRange(GetSameTiles(tile, Direction.DownLeft, piece, overflow, includeSurrounding));
                    break;
            }

            return tiles;
        }

        public Player GetCurrentPlayer()
        {
            return Players[Turn];
        }

        public void Play(Tile tile)
        {
            // Check if game is over
            if (IsGameOver)
                return;

            // Check for already placed tile
            if (tile.Piece.Symbol != Piece.EMPTY.Symbol)
                return;

            tile.Piece = GetCurrentPlayer().Piece;

            // Check for game over
            for (int i = 0; i <= 3; i++)
            {
                List<Tile> tiles = GetSameTiles(tile, (Orientation)i, tile.Piece);
                int samePieces = tiles.FindAll(t => t.Piece == tile.Piece).Count;
                bool hasBlank = tiles.Exists(t => t.Piece == Piece.EMPTY);

                if (samePieces + 1 == WINPIECES
                    && hasBlank)
                {
                    IsGameOver = true;
                    return;
                }
            }

            // Increment turn
            Turn = (Turn + 1) % Players.Count;

            // AI
            if (Turn == 1)
            {
                Play(new Gomoku.AI.GomokuAIv1().Play(this, GetCurrentPlayer()));
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
